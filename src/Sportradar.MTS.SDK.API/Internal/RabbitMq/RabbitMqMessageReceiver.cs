﻿/*
 * Copyright (C) Sportradar AG. See LICENSE for full license governing this code
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using log4net;
using Metrics;
using RabbitMQ.Client.Events;
using Sportradar.MTS.SDK.Common.Internal.Metrics;
using Sportradar.MTS.SDK.Common.Log;
using Sportradar.MTS.SDK.Entities.Enums;
using Sportradar.MTS.SDK.Entities.EventArguments;

namespace Sportradar.MTS.SDK.API.Internal.RabbitMq
{
    /// <summary>
    /// A <see cref="IRabbitMqMessageReceiver" /> implementation using RabbitMQ broker to deliver feed messages
    /// </summary>
    /// <seealso cref="Sportradar.MTS.SDK.API.Internal.RabbitMq.IRabbitMqMessageReceiver" />
    public sealed class RabbitMqMessageReceiver : IRabbitMqMessageReceiver, IHealthStatusProvider
    {
        /// <summary>
        /// A log4net.ILog used for feed traffic logging
        /// </summary>
        private static readonly ILog FeedLog = SdkLoggerFactory.GetLoggerForFeedTraffic(typeof(RabbitMqMessageReceiver));

        /// <summary>
        /// A <see cref="IRabbitMqConsumerChannel" /> representing a channel to the RabbitMQ broker
        /// </summary>
        private readonly IRabbitMqConsumerChannel _channel;

        /// <summary>
        /// Gets a value indicating whether the current <see cref="RabbitMqMessageReceiver" /> is currently opened;
        /// </summary>
        /// <value><c>true</c> if this instance is opened; otherwise, <c>false</c></value>
        public bool IsOpened => _channel.IsOpened;

        /// <summary>
        /// Event raised when the <see cref="IRabbitMqConsumerChannel" /> receives the message
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> MqMessageReceived;

        ///// <summary>
        ///// Event raised when the <see cref="IRabbitMqConsumerChannel" /> could not deserialize the received message
        ///// </summary>
        //public event EventHandler<MessageDeserializationFailedEventArgs> MqMessageDeserializationFailed;

        private readonly TicketResponseType _expectedTicketResponseType;

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMqMessageReceiver" /> class
        /// </summary>
        /// <param name="channel">A <see cref="IRabbitMqConsumerChannel" /> representing a consumer channel to the RabbitMQ broker</param>
        /// <param name="expectedResponseType">The type of the message receiver is expecting</param>
        public RabbitMqMessageReceiver(IRabbitMqConsumerChannel channel, TicketResponseType expectedResponseType)
        {
            Contract.Requires(channel != null);

            _channel = channel;
            _expectedTicketResponseType = expectedResponseType;
        }

        /// <summary>
        /// Defines invariant members of the class
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_channel != null);
        }

        /// <summary>
        /// Handles the message received event
        /// </summary>
        /// <param name="sender">The <see cref="object" /> representation of the event sender</param>
        /// <param name="eventArgs">A <see cref="BasicDeliverEventArgs" /> containing event information</param>
        private void consumer_OnReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            if (eventArgs?.Body == null || !eventArgs.Body.Any())
            {
                FeedLog.WarnFormat("A message with {0} body received. Aborting message processing", eventArgs?.Body == null ? "null" : "empty");
                return;
            }

            var stopwatch = Stopwatch.StartNew();

            var messageBody = Encoding.UTF8.GetString(eventArgs.Body);
            var correlationId = string.Empty;
            var additionalInfo = new Dictionary<string, string>();
            if (eventArgs.BasicProperties != null)
            {
                correlationId = eventArgs.BasicProperties.CorrelationId;

                if (eventArgs.BasicProperties.IsHeadersPresent())
                {
                    object obj;
                    if (eventArgs.BasicProperties.Headers.ContainsKey("receivedUtcTimestamp"))
                    {
                        if (eventArgs.BasicProperties.Headers.TryGetValue("receivedUtcTimestamp", out obj))
                        {
                            //var date = TicketHelper.UnixTimeToDateTime((long)obj);
                            additionalInfo.Add("receivedUtcTimestamp", obj.ToString());
                        }
                    }
                    if (eventArgs.BasicProperties.Headers.ContainsKey("validatedUtcTimestamp"))
                    {
                        if (eventArgs.BasicProperties.Headers.TryGetValue("validatedUtcTimestamp", out obj))
                        {
                            //var date = TicketHelper.UnixTimeToDateTime((long)obj);
                            additionalInfo.Add("validatedUtcTimestamp", obj.ToString());
                        }
                    }
                    if (eventArgs.BasicProperties.Headers.ContainsKey("respondedUtcTimestamp"))
                    {
                        if (eventArgs.BasicProperties.Headers.TryGetValue("respondedUtcTimestamp", out obj))
                        {
                            //var date = TicketHelper.UnixTimeToDateTime((long)obj);
                            additionalInfo.Add("respondedUtcTimestamp", obj.ToString());
                        }
                    }
                }
            }
            if (FeedLog.IsDebugEnabled)
            {
                FeedLog.Debug($"Message with correlationId: {correlationId} received. Msg: {messageBody}");
            }
            else
            {
                FeedLog.Info($"Message with correlationId: {correlationId} received.");
            }

            Metric.Context("RABBIT").Meter("RabbitMqMessageReceiver", Unit.Items).Mark(eventArgs.Exchange);

            RaiseMessageReceived(messageBody, eventArgs.RoutingKey, correlationId, additionalInfo);

            stopwatch.Stop();
            FeedLog.Info($"Message with correlationId: {correlationId} processed in {stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Raises the <see cref="MqMessageReceived" /> event
        /// </summary>
        /// <param name="body">The body of the message (json)</param>
        /// <param name="routingKey">The routing key</param>
        /// <param name="correlationId">The correlation id</param>
        /// <param name="additionalInfo">The additional information</param>
        private void RaiseMessageReceived(string body, string routingKey, string correlationId, IDictionary<string, string> additionalInfo)
        {
            Contract.Requires(!string.IsNullOrEmpty(body));
            
            MqMessageReceived?.Invoke(this, new MessageReceivedEventArgs(body, routingKey, correlationId, _expectedTicketResponseType, additionalInfo));
        }

        /// <summary>
        /// Opens the current instance
        /// </summary>
        public void Open()
        {
            _channel.Open();
            _channel.ChannelMessageReceived += consumer_OnReceived;
        }

        /// <summary>
        /// Opens the current <see cref="RabbitMqMessageReceiver" /> instance so it starts receiving messages
        /// </summary>
        /// <param name="routingKeys">A list of routing keys specifying which messages should the <see cref="RabbitMqMessageReceiver" /> deliver</param>
        public void Open(IEnumerable<string> routingKeys)
        {
            _channel.Open(routingKeys);
            _channel.ChannelMessageReceived += consumer_OnReceived;
        }

        /// <summary>
        /// Opens the current channel and binds the created queue to provided routing keys
        /// </summary>
        /// <param name="queueName">Name of the queue</param>
        /// <param name="routingKeys">A <see cref="IEnumerable{String}" /> specifying the routing keys of the constructed queue</param>
        public void Open(string queueName, IEnumerable<string> routingKeys)
        {
            _channel.Open(queueName, routingKeys);
            _channel.ChannelMessageReceived += consumer_OnReceived;
        }

        /// <summary>
        /// Closes the current <see cref="RabbitMqMessageReceiver" /> so it will no longer receive messages
        /// </summary>
        public void Close()
        {
            _channel.ChannelMessageReceived -= consumer_OnReceived;
            _channel.Close();
        }

        /// <summary>
        /// Registers the health check which will be periodically triggered
        /// </summary>
        public void RegisterHealthCheck()
        {
            HealthChecks.RegisterHealthCheck("RabbitMqMessageReceiver", new Func<HealthCheckResult>(StartHealthCheck));
        }

        /// <summary>
        /// Starts the health check and returns <see cref="HealthCheckResult" />
        /// </summary>
        /// <returns>HealthCheckResult</returns>
        public HealthCheckResult StartHealthCheck()
        {
            return HealthCheckResult.Healthy("RabbitMqMessageReceiver is operational.");
        }
    }
}
