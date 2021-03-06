﻿/*
 * Copyright (C) Sportradar AG. See LICENSE for full license governing this code
 */
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Net.Http;
using System.Runtime.Caching;
using log4net;
using Metrics;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using RabbitMQ.Client;
using Sportradar.MTS.SDK.API.Internal.Mappers;
using Sportradar.MTS.SDK.API.Internal.RabbitMq;
using Sportradar.MTS.SDK.API.Internal.Senders;
using Sportradar.MTS.SDK.Common.Internal;
using Sportradar.MTS.SDK.Common.Internal.Metrics;
using Sportradar.MTS.SDK.Common.Internal.Metrics.Reports;
using Sportradar.MTS.SDK.Common.Internal.Rest;
using Sportradar.MTS.SDK.Common.Log;
using Sportradar.MTS.SDK.Entities;
using Sportradar.MTS.SDK.Entities.Builders;
using Sportradar.MTS.SDK.Entities.Enums;
using Sportradar.MTS.SDK.Entities.Internal;
using Sportradar.MTS.SDK.Entities.Internal.Builders;
using Sportradar.MTS.SDK.Entities.Internal.Cache;
using Sportradar.MTS.SDK.Entities.Internal.REST;
using Sportradar.MTS.SDK.Entities.Internal.REST.Dto;

// ReSharper disable RedundantTypeArgumentsOfMethod

namespace Sportradar.MTS.SDK.API.Internal
{
    internal static class SdkUnityBootstrapper
    {
        private static readonly ILog Log = SdkLoggerFactory.GetLogger(typeof(SdkUnityBootstrapper));
        private const int RestConnectionFailureLimit = 3;
        private const int RestConnectionFailureTimeoutInSec = 12;
        private static string _environment;
        private static readonly CultureInfo DefaultCulture = new CultureInfo("en");

        public static void RegisterTypes(this IUnityContainer container, ISdkConfiguration userConfig)
        {
            Contract.Requires(container != null);
            Contract.Requires(userConfig != null);

            RegisterBaseClasses(container, userConfig);

            RegisterRabbitMqTypes(container, userConfig, _environment);

            RegisterTicketSenders(container);

            RegisterMarketDescriptionCache(container, userConfig);

            RegisterSdkStatisticsWriter(container, userConfig);
        }

        private static void RegisterBaseClasses(IUnityContainer container, ISdkConfiguration config)
        {
            container.RegisterInstance(config, new ContainerControlledLifetimeManager());

            container.RegisterType<ISdkConfigurationInternal, SdkConfigurationInternal>(new ContainerControlledLifetimeManager());
            var configInternal = new SdkConfigurationInternal(config);
            container.RegisterInstance(configInternal);

            if (configInternal.Host.Contains("tradinggate"))
            {
                _environment = "PROD";
            }
            else if (configInternal.Host.Contains("integration"))
            {
                _environment = "CI";
            }
            else
            {
                _environment = "CUSTOM";
            }

            container.RegisterType<IRabbitServer>(new ContainerControlledLifetimeManager());
            var rabbitServer = new RabbitServer(configInternal);
            container.RegisterInstance<IRabbitServer>(rabbitServer);

            container.RegisterType<ConnectionValidator, ConnectionValidator>(new ContainerControlledLifetimeManager());

            container.RegisterType<IConnectionFactory, ConfiguredConnectionFactory>(new ContainerControlledLifetimeManager());

            container.RegisterType<IChannelFactory, ChannelFactory>(new ContainerControlledLifetimeManager());

            container.RegisterInstance<ISequenceGenerator>(new IncrementalSequenceGenerator(), new ContainerControlledLifetimeManager());

            //register common types
            container.RegisterType<HttpClient, HttpClient>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor());

            var seed = (int)DateTime.Now.Ticks;
            var rand = new Random(seed);
            var value = rand.Next();
            Log.Info($"Initializing sequence generator with MinValue={value}, MaxValue={long.MaxValue}");
            container.RegisterType<ISequenceGenerator, IncrementalSequenceGenerator>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    (long)value,
                    long.MaxValue));

            container.RegisterType<HttpDataFetcher, HttpDataFetcher>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<HttpClient>(),
                    config.AccessToken ?? string.Empty,
                    RestConnectionFailureLimit,
                    RestConnectionFailureTimeoutInSec));

            container.RegisterType<LogHttpDataFetcher, LogHttpDataFetcher>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<HttpClient>(),
                    config.AccessToken ?? string.Empty,
                    new ResolvedParameter<ISequenceGenerator>(),
                    RestConnectionFailureLimit,
                    RestConnectionFailureTimeoutInSec));

            var logFetcher = container.Resolve<LogHttpDataFetcher>();
            container.RegisterInstance<IDataFetcher>(logFetcher, new ContainerControlledLifetimeManager());
            container.RegisterInstance<IDataPoster>(logFetcher, new ContainerControlledLifetimeManager());
        }

        private static void RegisterRabbitMqTypes(IUnityContainer container, ISdkConfiguration config, string environment)
        {
            Contract.Assume(container.Resolve<IChannelFactory>() != null);

            container.RegisterType<IRabbitMqChannelSettings, RabbitMqChannelSettings>(new ContainerControlledLifetimeManager());
            var rabbitMqChannelSettings = new RabbitMqChannelSettings(true, config.ExclusiveConsumer);
            container.RegisterInstance(rabbitMqChannelSettings);

            var rootExchangeName = config.VirtualHost.Replace("/", string.Empty);
            container.RegisterType<IMtsChannelSettings, MtsChannelSettings>(new ContainerControlledLifetimeManager());
            var mtsTicketChannelSettings = MtsChannelSettings.GetTicketChannelSettings(rootExchangeName, config.Username, config.NodeId, environment);
            var mtsTicketCancelChannelSettings = MtsChannelSettings.GetTicketCancelChannelSettings(rootExchangeName, config.Username, config.NodeId, environment);
            var mtsTicketAckChannelSettings = MtsChannelSettings.GetTicketAckChannelSettings(rootExchangeName, config.Username, config.NodeId, environment);
            var mtsTicketCancelAckChannelSettings = MtsChannelSettings.GetTicketCancelAckChannelSettings(rootExchangeName, config.Username, config.NodeId, environment);
            var mtsTicketReofferCancelChannelSettings = MtsChannelSettings.GetTicketReofferCancelChannelSettings(rootExchangeName, config.Username, config.NodeId, environment);
            var mtsTicketCashoutChannelSettings = MtsChannelSettings.GetTicketCashoutChannelSettings(rootExchangeName, config.Username, config.NodeId, environment);

            var mtsTicketResponseChannelSettings = MtsChannelSettings.GetTicketResponseChannelSettings(rootExchangeName, config.Username, config.NodeId, environment);
            var mtsTicketCancelResponseChannelSettings = MtsChannelSettings.GetTicketCancelResponseChannelSettings(rootExchangeName, config.Username, config.NodeId, environment);
            var mtsTicketCashoutResponseChannelSettings = MtsChannelSettings.GetTicketCashoutResponseChannelSettings(rootExchangeName, config.Username, config.NodeId, environment);

            container.RegisterInstance<IMtsChannelSettings>("TicketChannelSettings", mtsTicketChannelSettings);
            container.RegisterInstance<IMtsChannelSettings>("TicketCancelChannelSettings", mtsTicketCancelChannelSettings);
            container.RegisterInstance<IMtsChannelSettings>("TicketAckChannelSettings", mtsTicketAckChannelSettings);
            container.RegisterInstance<IMtsChannelSettings>("TicketCancelAckChannelSettings", mtsTicketCancelAckChannelSettings);
            container.RegisterInstance<IMtsChannelSettings>("TicketReofferCancelChannelSettings", mtsTicketReofferCancelChannelSettings);
            container.RegisterInstance<IMtsChannelSettings>("TicketCashoutChannelSettings", mtsTicketCashoutChannelSettings);

            container.RegisterInstance<IMtsChannelSettings>("TicketResponseChannelSettings", mtsTicketResponseChannelSettings);
            container.RegisterInstance<IMtsChannelSettings>("TicketCancelResponseChannelSettings", mtsTicketCancelResponseChannelSettings);
            container.RegisterInstance<IMtsChannelSettings>("TicketCashoutResponseChannelSettings", mtsTicketCashoutResponseChannelSettings);

            container.RegisterType<IRabbitMqConsumerChannel, RabbitMqConsumerChannel>(new HierarchicalLifetimeManager());
            var ticketResponseConsumerChannel = new RabbitMqConsumerChannel(container.Resolve<IChannelFactory>(),
                                                                            container.Resolve<IMtsChannelSettings>("TicketResponseChannelSettings"),
                                                                            container.Resolve<IRabbitMqChannelSettings>());
            var ticketCancelResponseConsumerChannel = new RabbitMqConsumerChannel(container.Resolve<IChannelFactory>(),
                                                                            container.Resolve<IMtsChannelSettings>("TicketCancelResponseChannelSettings"),
                                                                            container.Resolve<IRabbitMqChannelSettings>());
            var ticketCashoutResponseConsumerChannel = new RabbitMqConsumerChannel(container.Resolve<IChannelFactory>(),
                                                                            container.Resolve<IMtsChannelSettings>("TicketCashoutResponseChannelSettings"),
                                                                            container.Resolve<IRabbitMqChannelSettings>());
            container.RegisterInstance<IRabbitMqConsumerChannel>("TicketConsumerChannel", ticketResponseConsumerChannel);
            container.RegisterInstance<IRabbitMqConsumerChannel>("TicketCancelConsumerChannel", ticketCancelResponseConsumerChannel);
            container.RegisterInstance<IRabbitMqConsumerChannel>("TicketCashoutConsumerChannel", ticketCashoutResponseConsumerChannel);

            container.RegisterType<IRabbitMqMessageReceiver, RabbitMqMessageReceiver>(new HierarchicalLifetimeManager());
            container.RegisterInstance<IRabbitMqMessageReceiver>("TicketResponseMessageReceiver", new RabbitMqMessageReceiver(ticketResponseConsumerChannel, TicketResponseType.Ticket));
            container.RegisterInstance<IRabbitMqMessageReceiver>("TicketCancelResponseMessageReceiver", new RabbitMqMessageReceiver(ticketCancelResponseConsumerChannel, TicketResponseType.TicketCancel));
            container.RegisterInstance<IRabbitMqMessageReceiver>("TicketCashoutResponseMessageReceiver", new RabbitMqMessageReceiver(ticketCashoutResponseConsumerChannel, TicketResponseType.TicketCashout));

            container.RegisterType<IRabbitMqPublisherChannel, RabbitMqPublisherChannel>(new HierarchicalLifetimeManager());
            var ticketPC = new RabbitMqPublisherChannel(container.Resolve<IChannelFactory>(),
                                                        mtsTicketChannelSettings,
                                                        container.Resolve<IRabbitMqChannelSettings>());
            var ticketAckPC = new RabbitMqPublisherChannel(container.Resolve<IChannelFactory>(),
                                                        mtsTicketAckChannelSettings,
                                                        container.Resolve<IRabbitMqChannelSettings>());
            var ticketCancelPC = new RabbitMqPublisherChannel(container.Resolve<IChannelFactory>(),
                                                        mtsTicketCancelChannelSettings,
                                                        container.Resolve<IRabbitMqChannelSettings>());
            var ticketCancelAckPC = new RabbitMqPublisherChannel(container.Resolve<IChannelFactory>(),
                                                        mtsTicketCancelAckChannelSettings,
                                                        container.Resolve<IRabbitMqChannelSettings>());
            var ticketReofferCancelPC = new RabbitMqPublisherChannel(container.Resolve<IChannelFactory>(),
                                                        mtsTicketReofferCancelChannelSettings,
                                                        container.Resolve<IRabbitMqChannelSettings>());
            var ticketCashoutPC = new RabbitMqPublisherChannel(container.Resolve<IChannelFactory>(),
                                                        mtsTicketCashoutChannelSettings,
                                                        container.Resolve<IRabbitMqChannelSettings>());
            container.RegisterInstance<IRabbitMqPublisherChannel>("TicketPublisherChannel", ticketPC);
            container.RegisterInstance<IRabbitMqPublisherChannel>("TicketAckPublisherChannel", ticketAckPC);
            container.RegisterInstance<IRabbitMqPublisherChannel>("TicketCancelPublisherChannel", ticketCancelPC);
            container.RegisterInstance<IRabbitMqPublisherChannel>("TicketCancelAckPublisherChannel", ticketCancelAckPC);
            container.RegisterInstance<IRabbitMqPublisherChannel>("TicketReofferCancelPublisherChannel", ticketReofferCancelPC);
            container.RegisterInstance<IRabbitMqPublisherChannel>("TicketCashoutPublisherChannel", ticketCashoutPC);
        }

        private static void RegisterTicketSenders(IUnityContainer container)
        {
            var ticketCache = new ConcurrentDictionary<string, TicketCacheItem>();

            container.RegisterType<ITicketSender>(new ContainerControlledLifetimeManager());
            var ticketSender = new TicketSender(new TicketMapper(),
                                                container.Resolve<IRabbitMqPublisherChannel>("TicketPublisherChannel"),
                                                ticketCache,
                                                container.Resolve<IMtsChannelSettings>("TicketChannelSettings"),
                                                container.Resolve<IRabbitMqChannelSettings>().PublishQueueTimeoutInSec);
            var ticketAckSender = new TicketAckSender(new TicketAckMapper(),
                                                container.Resolve<IRabbitMqPublisherChannel>("TicketAckPublisherChannel"),
                                                ticketCache,
                                                container.Resolve<IMtsChannelSettings>("TicketAckChannelSettings"),
                                                container.Resolve<IRabbitMqChannelSettings>().PublishQueueTimeoutInSec);
            var ticketCancelSender = new TicketCancelSender(new TicketCancelMapper(),
                                                container.Resolve<IRabbitMqPublisherChannel>("TicketCancelPublisherChannel"),
                                                ticketCache,
                                                container.Resolve<IMtsChannelSettings>("TicketCancelChannelSettings"),
                                                container.Resolve<IRabbitMqChannelSettings>().PublishQueueTimeoutInSec);
            var ticketCancelAckSender = new TicketCancelAckSender(new TicketCancelAckMapper(),
                                                container.Resolve<IRabbitMqPublisherChannel>("TicketCancelAckPublisherChannel"),
                                                ticketCache,
                                                container.Resolve<IMtsChannelSettings>("TicketCancelAckChannelSettings"),
                                                container.Resolve<IRabbitMqChannelSettings>().PublishQueueTimeoutInSec);
            var ticketReofferCancelSender = new TicketReofferCancelSender(new TicketReofferCancelMapper(),
                                                container.Resolve<IRabbitMqPublisherChannel>("TicketReofferCancelPublisherChannel"),
                                                ticketCache,
                                                container.Resolve<IMtsChannelSettings>("TicketReofferCancelChannelSettings"),
                                                container.Resolve<IRabbitMqChannelSettings>().PublishQueueTimeoutInSec);
            var ticketCashoutSender = new TicketCashoutSender(new TicketCashoutMapper(),
                                                container.Resolve<IRabbitMqPublisherChannel>("TicketCashoutPublisherChannel"),
                                                ticketCache,
                                                container.Resolve<IMtsChannelSettings>("TicketCashoutChannelSettings"),
                                                container.Resolve<IRabbitMqChannelSettings>().PublishQueueTimeoutInSec);
            container.RegisterInstance<ITicketSender>("TicketSender", ticketSender);
            container.RegisterInstance<ITicketSender>("TicketAckSender", ticketAckSender);
            container.RegisterInstance<ITicketSender>("TicketCancelSender", ticketCancelSender);
            container.RegisterInstance<ITicketSender>("TicketCancelAckSender", ticketCancelAckSender);
            container.RegisterInstance<ITicketSender>("TicketReofferCancelSender", ticketReofferCancelSender);
            container.RegisterInstance<ITicketSender>("TicketCashoutSender", ticketCashoutSender);

            var senders = new Dictionary<SdkTicketType, ITicketSender>
            {
                {SdkTicketType.Ticket, container.Resolve<ITicketSender>("TicketSender")},
                {SdkTicketType.TicketAck, container.Resolve<ITicketSender>("TicketAckSender")},
                {SdkTicketType.TicketCancel, container.Resolve<ITicketSender>("TicketCancelSender")},
                {SdkTicketType.TicketCancelAck, container.Resolve<ITicketSender>("TicketCancelAckSender")},
                {SdkTicketType.TicketReofferCancel, container.Resolve<ITicketSender>("TicketReofferCancelSender")},
                {SdkTicketType.TicketCashout, container.Resolve<ITicketSender>("TicketCashoutSender")},
            };

            var senderFactory = new TicketSenderFactory(senders);
            container.RegisterType<ITicketSenderFactory, TicketSenderFactory>(new ContainerControlledLifetimeManager());
            container.RegisterInstance(senderFactory);

            var entityMapper = new EntitiesMapper(ticketAckSender, ticketCancelAckSender);
            container.RegisterType<EntitiesMapper>(new ContainerControlledLifetimeManager());
            container.RegisterInstance(entityMapper);
        }

        private static void RegisterSdkStatisticsWriter(IUnityContainer container, ISdkConfiguration config)
        {
            var x = container.ResolveAll<IRabbitMqMessageReceiver>();
            var statusProviders = new List<IHealthStatusProvider>();
            x.ForEach(f=>statusProviders.Add((IHealthStatusProvider)f));

            container.RegisterType<MetricsReporter, MetricsReporter>(new ContainerControlledLifetimeManager(),
                                                                     new InjectionConstructor(MetricsReportPrintMode.Normal, 2, true));

            var metricReporter = container.Resolve<MetricsReporter>();

            Metric.Config.WithAllCounters().WithReporting(rep => rep.WithReport(metricReporter, TimeSpan.FromSeconds(config.StatisticsTimeout)));

            container.RegisterInstance(metricReporter, new ContainerControlledLifetimeManager());

            foreach (var sp in statusProviders)
            {
                sp.RegisterHealthCheck();
            }
        }

        private static void RegisterMarketDescriptionCache(IUnityContainer container, ISdkConfiguration config)
        {
            var configInternal = container.Resolve<ISdkConfigurationInternal>();

            // Invariant market description provider
            container.RegisterType<IDeserializer<market_descriptions>, Deserializer<market_descriptions>>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISingleTypeMapperFactory<market_descriptions, IEnumerable<MarketDescriptionDTO>>, MarketDescriptionsMapperFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataProvider<IEnumerable<MarketDescriptionDTO>>,
                DataProvider<market_descriptions, IEnumerable<MarketDescriptionDTO>>>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    configInternal.ApiHost + "/v1/descriptions/{0}/markets.xml?include_mappings=true",
                    new ResolvedParameter<IDataFetcher>(),
                    new ResolvedParameter<IDeserializer<market_descriptions>>(),
                    new ResolvedParameter<ISingleTypeMapperFactory<market_descriptions, IEnumerable<MarketDescriptionDTO>>>()));

            // Cache for invariant markets
            container.RegisterInstance(
                "InvariantMarketDescriptionCache_Cache",
                new MemoryCache("invariantMarketsDescriptionsCache"),
                new ContainerControlledLifetimeManager());

            // Timer for invariant markets
            container.RegisterType<ITimer, SdkTimer>(
                "InvariantMarketCacheTimer",
                new HierarchicalLifetimeManager(),
                new InjectionConstructor(
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromHours(6)));

            // Invariant market cache
            container.RegisterType<IMarketDescriptionCache, MarketDescriptionCache>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<MemoryCache>("InvariantMarketDescriptionCache_Cache"),
                    new ResolvedParameter<IDataProvider<IEnumerable<MarketDescriptionDTO>>>(),
                    new List<CultureInfo> { DefaultCulture },
                    config.AccessToken ?? string.Empty,
                    TimeSpan.FromHours(4),
                    new CacheItemPolicy { SlidingExpiration = TimeSpan.FromDays(1) }));

            container.RegisterType<IMarketDescriptionProvider, MarketDescriptionProvider>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<IMarketDescriptionCache>(),
                    new List<CultureInfo> { DefaultCulture }));

            container.RegisterType<IBuilderFactory, BuilderFactory>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<ISdkConfigurationInternal>(),
                    new ResolvedParameter<IMarketDescriptionProvider>()));
        }
    }
}
