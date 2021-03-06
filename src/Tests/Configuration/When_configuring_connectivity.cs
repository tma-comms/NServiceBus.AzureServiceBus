namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using System;
    using Microsoft.ServiceBus.Messaging;
    using AzureServiceBus;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_connectivity
    {
        [Test]
        public void Should_be_able_to_set_messaging_factory_settings_factory_method()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Func<string, MessagingFactorySettings> registeredFactory = s => new MessagingFactorySettings();

            var connectivitySettings = extensions.MessagingFactories().MessagingFactorySettingsFactory(registeredFactory);

            Assert.AreEqual(registeredFactory, connectivitySettings.GetSettings().Get<Func<string, MessagingFactorySettings>>(WellKnownConfigurationKeys.Connectivity.MessagingFactories.MessagingFactorySettingsFactory));
        }

        [Test]
        public void Should_be_able_to_set_number_of_messaging_factories_per_namespace()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var connectivitySettings = extensions.MessagingFactories().NumberOfMessagingFactoriesPerNamespace(4);

            Assert.AreEqual(4, connectivitySettings.GetSettings().Get<int>(WellKnownConfigurationKeys.Connectivity.MessagingFactories.NumberOfMessagingFactoriesPerNamespace));
        }

        [Test]
        public void Should_be_able_to_set_number_of_clients_per_entity()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.NumberOfClientsPerEntity(4);

            Assert.AreEqual(4, settings.Get<int>(WellKnownConfigurationKeys.Connectivity.NumberOfClientsPerEntity));
        }

        public void Should_be_able_to_set_whether_send_via_receive_queue_should_be_used()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.SendViaReceiveQueue(false);

            Assert.AreEqual(false, settings.Get<bool>(WellKnownConfigurationKeys.Connectivity.SendViaReceiveQueue));
        }
    }
}