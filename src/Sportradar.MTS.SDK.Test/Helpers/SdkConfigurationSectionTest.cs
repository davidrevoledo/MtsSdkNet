﻿/*
 * Copyright (C) Sportradar AG. See LICENSE for full license governing this code
 */

using Sportradar.MTS.SDK.Entities.Enums;
using Sportradar.MTS.SDK.Entities.Internal;

namespace Sportradar.MTS.SDK.Test.Helpers
{
    public class SdkConfigurationSectionTest : ISdkConfigurationSection
    {
        /// <summary>
        /// Gets the username used to connect to MTS
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// Gets the password used to connect to MTS
        /// </summary>
        public string Password { get; }

        /// <summary>
        /// Gets the hostname of the broker
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// Gets the port number used to connect to the broker
        /// </summary>
        /// <remarks>Port should be chosen through the useSsl property. Manually setting port number should be used only when non-default port is required</remarks>
        public int Port { get; }

        /// <summary>
        /// Gets the virtual host name
        /// </summary>
        public string VirtualHost { get; }

        /// <summary>
        /// Gets the node id for this instance of sdk
        /// </summary>
        public int NodeId { get; }

        /// <summary>
        /// Gets a value specifying whether the connection to AMQP broker should use SSL encryption
        /// </summary>
        public bool UseSsl { get; }

        /// <summary>
        /// Gets the default sender bookmakerId 
        /// </summary>
        public int BookmakerId { get; }

        /// <summary>
        /// Gets the default sender limitId
        /// </summary>
        public int LimitId { get; }

        /// <summary>
        /// Gets the default sender currency sign (3 or 4 letter)
        /// </summary>
        public string Currency { get; }

        /// <summary>
        /// Gets the default sender channel (see <see cref="SenderChannel"/> for possible values)
        /// </summary>
        public SenderChannel? Channel { get; }

        /// <summary>
        /// Gets the access token for the UF feed (only necessary if UF selections will be build)
        /// </summary>
        public string AccessToken { get; }

        /// <summary>
        /// This value is used to indicate if the sdk should add market specifiers for specific markets. Only used when building selection using UnifiedOdds ids. (default: true)
        /// </summary>
        /// <remarks>If this is set to true and the user uses UOF markets, when there are special cases (market 215, or $score in SOV/SBV template), sdk automatically tries to add appropriate specifier; if set to false, user will need to add this manually</remarks>
        public bool ProvideAdditionalMarketSpecifiers { get; }

        /// <summary>
        /// Is statistics collecting enabled
        /// </summary>
        public bool StatisticsEnabled { get; }

        /// <summary>
        /// Gets the timeout for automatically collecting statistics
        /// </summary>
        public int StatisticsTimeout { get; }

        /// <summary>
        /// Gets the limit of records for automatically writing statistics
        /// </summary>
        public int StatisticsRecordLimit { get; }

        /// <summary>
        /// Gets the file path to the configuration file for the log4net repository used by the SDK
        /// </summary>
        public string SdkLogConfigPath { get; }

        /// <summary>
        /// Should the rabbit consumer be exclusive
        /// </summary>
        public bool ExclusiveConsumer { get; }

        public SdkConfigurationSectionTest(string username, string password, string host, int port, string virtualHost, int nodeId, bool useSsl, int bookmakerId, int limitId, string currency, SenderChannel? channel, string accessToken, bool provideAdditionalMarketSpecifiers, bool statisticsEnabled, int statisticsTimeout, int statisticsRecordLimit, string sdkLogConfigPath, bool exclusiveConsumer)
        {
            Username = username;
            Password = password;
            Host = host;
            Port = port;
            VirtualHost = virtualHost;
            NodeId = nodeId;
            UseSsl = useSsl;
            BookmakerId = bookmakerId;
            LimitId = limitId;
            Currency = currency;
            Channel = channel;
            AccessToken = accessToken;
            ProvideAdditionalMarketSpecifiers = provideAdditionalMarketSpecifiers;
            StatisticsEnabled = statisticsEnabled;
            StatisticsTimeout = statisticsTimeout;
            StatisticsRecordLimit = statisticsRecordLimit;
            SdkLogConfigPath = sdkLogConfigPath;
            ExclusiveConsumer = exclusiveConsumer;
        }

        public static SdkConfigurationSectionTest Create()
        {
            return new SdkConfigurationSectionTest(
                                                   username: "username",
                                                   password: "password",
                                                   host: "host",
                                                   port: 5671,
                                                   virtualHost: "/test",
                                                   nodeId: 1,
                                                   useSsl: true,
                                                   bookmakerId: 111,
                                                   limitId: 1,
                                                   currency: "EUR",
                                                   channel: SenderChannel.Internet,
                                                   accessToken: "UfAccessToken",
                                                   provideAdditionalMarketSpecifiers: true,
                                                   statisticsEnabled: true,
                                                   statisticsTimeout: 60,
                                                   statisticsRecordLimit: 1000,
                                                   sdkLogConfigPath: string.Empty,
                                                   exclusiveConsumer: true);
        }
    }
}
