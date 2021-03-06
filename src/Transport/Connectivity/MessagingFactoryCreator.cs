namespace NServiceBus.AzureServiceBus
{
    using System;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Settings;

    class MessagingFactoryCreator : ICreateMessagingFactories
    {
        IManageNamespaceManagerLifeCycle namespaceManagers;
        Func<string, MessagingFactorySettings> settingsFactory;
        ReadOnlySettings settings;

        public MessagingFactoryCreator(IManageNamespaceManagerLifeCycle namespaceManagers, ReadOnlySettings settings)
        {
            this.namespaceManagers = namespaceManagers;
            this.settings = settings;

            if (settings.HasExplicitValue(WellKnownConfigurationKeys.Connectivity.MessagingFactories.MessagingFactorySettingsFactory))
            {
                settingsFactory = settings.Get<Func<string, MessagingFactorySettings>>(WellKnownConfigurationKeys.Connectivity.MessagingFactories.MessagingFactorySettingsFactory);
            }
            else
            {
                settingsFactory = namespaceName =>
                {
                    var namespaceManager = this.namespaceManagers.Get(namespaceName);

                    var s = new MessagingFactorySettings
                    {
                        TokenProvider = namespaceManager.Settings.TokenProvider,
                        NetMessagingTransportSettings =
                        {
                            BatchFlushInterval = settings.Get<TimeSpan>(WellKnownConfigurationKeys.Connectivity.MessagingFactories.BatchFlushInterval)
                        }
                    };

                    return s;
                };
            }
        }

        public IMessagingFactory Create(string namespaceName)
        {
            var namespaceManager = namespaceManagers.Get(namespaceName);
            var factorySettings = settingsFactory(namespaceName);
            var inner = MessagingFactory.Create(namespaceManager.Address, factorySettings);
            if (settings.HasExplicitValue(WellKnownConfigurationKeys.Connectivity.MessagingFactories.RetryPolicy))
            {
                inner.RetryPolicy = settings.Get<RetryPolicy>(WellKnownConfigurationKeys.Connectivity.MessagingFactories.RetryPolicy);
            }
            return new MessagingFactoryAdapter(inner);
        }

    }
}