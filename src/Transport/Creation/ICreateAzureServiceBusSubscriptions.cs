﻿namespace NServiceBus.AzureServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    public interface ICreateAzureServiceBusSubscriptions
    {
        Task<SubscriptionDescription> Create(string topicPath, string subscriptionName, SubscriptionMetadata metadata, string sqlFilter, INamespaceManager namespaceManager, string forwardTo);
    }
}