﻿/*
 * Copyright (C) Sportradar AG. See LICENSE for full license governing this code
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Metrics.Utils;
using Sportradar.MTS.SDK.API.Internal.RabbitMq;
using Sportradar.MTS.SDK.Common.Internal;

namespace Sportradar.MTS.SDK.API.Internal
{
    internal class MtsChannelSettings : IMtsChannelSettings
    {
        //reoffer-cancel-2.0-schema.json has no response (cancel.reoffer)
        //account has to have cashout enabled and it works only for live events

        public string ChannelQueueName { get; }

        public string ExchangeName { get; }

        public ExchangeType ExchangeType { get; }

        public IEnumerable<string> RoutingKeys { get; }

        public IReadOnlyDictionary<string, object> HeaderProperties { get; }

        public string ReplyToRoutingKey { get; }

        public string ConsumerTag { get; }

        public string PublishRoutingKey { get; }

        private MtsChannelSettings(string queueName, string exchangeName, ExchangeType exchangeType, string routingKey, IReadOnlyDictionary<string, object> headerProperties, string replyToRoutingKey, string environment)
        {
            ChannelQueueName = queueName;
            ExchangeName = exchangeName;
            ExchangeType = exchangeType;
            RoutingKeys = string.IsNullOrEmpty(routingKey) ? null : new List<string> { routingKey };
            PublishRoutingKey = routingKey;
            HeaderProperties = headerProperties;
            ReplyToRoutingKey = replyToRoutingKey;
            var systemStartTime = DateTime.Now.AddMilliseconds(-Environment.TickCount);
            ConsumerTag = $"tag_{environment}|NET|{SdkInfo.GetVersion()}|{DateTime.Now:yyyyMMddHHmm}|{systemStartTime.ToUnixTime()}|{Process.GetCurrentProcess().Id}";
        }

        internal MtsChannelSettings(string queueName, string exchangeName, ExchangeType exchangeType, IEnumerable<string> routingKeys, IReadOnlyDictionary<string, object> headerProperties, string environment)
        {
            ChannelQueueName = queueName;
            ExchangeName = exchangeName;
            ExchangeType = exchangeType;
            if (routingKeys != null)
            {
                var enumerable = routingKeys as IList<string> ?? routingKeys.ToList();
                RoutingKeys = enumerable;
                PublishRoutingKey = enumerable.First();
            }
            HeaderProperties = headerProperties;
            var systemStartTime = DateTime.Now.AddMilliseconds(-Environment.TickCount);
            ConsumerTag = $"tag_{environment}|NET|{SdkInfo.GetVersion()}|{DateTime.Now:yyyyMMddHHmm}|{systemStartTime.ToUnixTime()}|{Process.GetCurrentProcess().Id}";
        }

        public static IMtsChannelSettings GetTicketChannelSettings(string rootExchangeName, string username, int nodeId, string environment)
        {
            var headers = new Dictionary<string, object> { { "replyRoutingKey", $"node{nodeId}.ticket.confirm" } };
            return new MtsChannelSettings(null,
                                          $"{rootExchangeName}-Submit",
                                          ExchangeType.Fanout,
                                          $"{username}-Confirm-node{nodeId}",
                                          new ReadOnlyDictionary<string, object>(headers),
                                          headers.First().Value.ToString(),
                                          environment);
        }

        public static IMtsChannelSettings GetTicketResponseChannelSettings(string rootExchangeName, string username, int nodeId, string environment)
        {
            return new MtsChannelSettings($"{username}-Confirm-node{nodeId}",
                                          $"{rootExchangeName}-Confirm",
                                          ExchangeType.Topic,
                                          $"node{nodeId}.ticket.confirm",
                                          null,
                                          null,
                                          environment);
        }

        public static IMtsChannelSettings GetTicketAckChannelSettings(string rootExchangeName, string username, int nodeId, string environment)
        {
            //var headers = new Dictionary<string, object> { { "routing-key", $"{username}-Confirm-node{nodeId}" } };
            return new MtsChannelSettings(null,
                                          $"{rootExchangeName}-Ack",
                                          ExchangeType.Topic,
                                          "ack.ticket",
                                          null,
                                          null,
                                          environment);
        }

        public static IMtsChannelSettings GetTicketCancelChannelSettings(string rootExchangeName, string username, int nodeId, string environment)
        {
            var headers = new Dictionary<string, object> { { "replyRoutingKey", $"node{nodeId}.cancel.confirm" } };
            return new MtsChannelSettings(null,
                                          $"{rootExchangeName}-Control",
                                          ExchangeType.Topic,
                                          $"{username}-Reply-node{nodeId}",
                                          new ReadOnlyDictionary<string, object>(headers),
                                          headers.First().Value.ToString(),
                                          environment);
        }

        public static IMtsChannelSettings GetTicketCancelResponseChannelSettings(string rootExchangeName, string username, int nodeId, string environment)
        {
            return new MtsChannelSettings($"{username}-Reply-node{nodeId}",
                                          $"{rootExchangeName}-Reply",
                                          ExchangeType.Topic,
                                          $"node{nodeId}.cancel.confirm",
                                          null,
                                          null,
                                          environment);
        }

        public static IMtsChannelSettings GetTicketCancelAckChannelSettings(string rootExchangeName, string username, int nodeId, string environment)
        {
            //var headers = new Dictionary<string, object> { { "routing-key", $"{username}-Reply-node{nodeId}" } };
            return new MtsChannelSettings(null,
                                          $"{rootExchangeName}-Ack",
                                          ExchangeType.Topic,
                                          "ack.cancel",
                                          null,
                                          null,
                                          environment);
        }

        public static IMtsChannelSettings GetTicketReofferChannelSettings(string rootExchangeName, string username, int nodeId)
        {
            throw new InvalidProgramException("TicketReoffer must be send as normal ticket.");
        }

        public static IMtsChannelSettings GetTicketReofferCancelChannelSettings(string rootExchangeName, string username, int nodeId, string environment)
        {
            var headers = new Dictionary<string, object> { { "routing-key", $"{username}-Reply-node{nodeId}" } };
            return new MtsChannelSettings(null,
                                          $"{rootExchangeName}-Control",
                                          ExchangeType.Topic,
                                          "cancel.reoffer",
                                          new ReadOnlyDictionary<string, object>(headers),
                                          null,
                                          environment);
        }

        public static IMtsChannelSettings GetTicketCashoutChannelSettings(string rootExchangeName, string username, int nodeId, string environment)
        {
            var headers = new Dictionary<string, object> { { "replyRoutingKey", $"node{nodeId}.ticket.cashout" } };
            return new MtsChannelSettings(null,
                                          $"{rootExchangeName}-Control",
                                          ExchangeType.Topic,
                                          "ticket.cashout",
                                          new ReadOnlyDictionary<string, object>(headers),
                                          headers.First().Value.ToString(),
                                          environment);
        }

        public static IMtsChannelSettings GetTicketCashoutResponseChannelSettings(string rootExchangeName, string username, int nodeId, string environment)
        {
            return new MtsChannelSettings($"{username}-Reply-cashout-node{nodeId}",
                                          $"{rootExchangeName}-Reply",
                                          ExchangeType.Topic,
                                          $"node{nodeId}.ticket.cashout",
                                          null,
                                          null,
                                          environment);
        }
    }
}
