﻿namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using AzureServiceBus;
    using DelayedDelivery;
    using Performance.TimeToBeReceived;
    using Routing;
    using Settings;
    using Transports;

    class AzureServiceBusTransportInfrastructure : TransportInfrastructure
    {
        ITopology topology;
        SettingsHolder settings;

        public AzureServiceBusTransportInfrastructure(ITopology topology, SettingsHolder settings)
        {
            this.topology = topology;
            this.settings = settings;
        }

        public override EndpointInstance BindToLocalEndpoint(EndpointInstance instance)
        {
            return instance;
        }

        public override string ToTransportAddress(LogicalAddress logicalAddress)
        {
            return logicalAddress.ToString();
        }

        public override TransportReceiveInfrastructure ConfigureReceiveInfrastructure()
        {
            return new TransportReceiveInfrastructure(
                topology.GetMessagePumpFactory(),
                topology.GetQueueCreatorFactory(),
                () => topology.RunPreStartupChecks());
        }

        public override TransportSendInfrastructure ConfigureSendInfrastructure()
        {
            return new TransportSendInfrastructure(
                topology.GetDispatcherFactory(),
                () => topology.RunPreStartupChecks());
        }

        public override TransportSubscriptionInfrastructure ConfigureSubscriptionInfrastructure()
        {
            return new TransportSubscriptionInfrastructure(topology.GetSubscriptionManagerFactory());
        }

        public override IEnumerable<Type> DeliveryConstraints => new List<Type> { typeof(DelayDeliveryWith), typeof(DoNotDeliverBefore), typeof(DiscardIfNotReceivedBefore) };

        public override TransportTransactionMode TransactionMode => settings.SupportedTransactionMode();

        public override OutboundRoutingPolicy OutboundRoutingPolicy => topology.GetOutboundRoutingPolicy();
    }
}