﻿/*
 * Copyright (C) Sportradar AG. See LICENSE for full license governing this code
 */
using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Sportradar.MTS.SDK.API.Contracts;
using Sportradar.MTS.SDK.Common;
using Sportradar.MTS.SDK.Entities.EventArguments;

namespace Sportradar.MTS.SDK.API.Internal.RabbitMq
{
    /// <summary>
    /// Represents a contract implemented by classes used to connect to rabbit mq broker
    /// </summary>
    [ContractClass(typeof(RabbitMqPublisherChannelContract))]
    public interface IRabbitMqPublisherChannel : IOpenable
    {
        /// <summary>
        /// Raised when the attempt to publish message failed
        /// </summary>
        event EventHandler<MessagePublishFailedEventArgs> MqMessagePublishFailed;

        /// <summary>
        /// Publishes the specified message
        /// </summary>
        /// <param name="msg">The message to be published</param>
        /// <param name="routingKey">The routing key to be set while publishing</param>
        /// <param name="correlationId">The correlation identifier</param>
        /// <param name="replyRoutingKey">The reply routing key</param>
        /// <returns>A <see cref="IMqPublishResult"/></returns>
        IMqPublishResult Publish(byte[] msg, string routingKey, string correlationId, string replyRoutingKey);

        /// <summary>
        /// Asynchronously publishes the specified message
        /// </summary>
        /// <param name="msg">The message to be published</param>
        /// <param name="routingKey">The routing key to be set while publishing</param>
        /// <param name="correlationId">The correlation identifier</param>
        /// <param name="replyRoutingKey">The reply routing key</param>
        /// <returns>A <see cref="IMqPublishResult"/></returns>
        Task<IMqPublishResult> PublishAsync(byte[] msg, string routingKey, string correlationId, string replyRoutingKey);
    }
}
