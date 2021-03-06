namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using System;
    using AzureServiceBus;
    using AzureServiceBus.Addressing;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_validation
    {
        SettingsHolder settingsHolder;
        TransportExtensions<AzureServiceBusTransport> extensions;

        [SetUp]
        public void SetUp()
        {
            settingsHolder = new SettingsHolder();
            extensions = new TransportExtensions<AzureServiceBusTransport>(settingsHolder);
        }

        [Test]
        public void Should_be_able_to_set_the_validation_strategy()
        {
            var validationSettings = extensions.Validation().UseStrategy<MyValidationStrategy>();

            Assert.AreEqual(typeof(MyValidationStrategy), validationSettings.GetSettings().Get<Type>(WellKnownConfigurationKeys.Topology.Addressing.Validation.Strategy));
        }

        class MyValidationStrategy : IValidationStrategy
        {
            public bool IsValid(string entityPath, EntityType entityType)
            {
                throw new NotImplementedException(); // not relevant for test
            }
        }


        [Test]
        public void Should_be_able_to_set_queue_path_maximum_length()
        {
            var validationSettings = extensions.Validation().UseQueuePathMaximumLength(10);

            Assert.AreEqual(10, validationSettings.GetSettings().Get<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.QueuePathMaximumLength));
        }

        [Test]
        public void Should_be_able_to_set_topic_path_maximum_length()
        {
            var validationSettings = extensions.Validation().UseTopicPathMaximumLength(20);

            Assert.AreEqual(20, validationSettings.GetSettings().Get<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.TopicPathMaximumLength));
        }

        [Test]
        public void Should_be_able_to_set_subscription_path_maximum_length()
        {
            var validationSettings = extensions.Validation().UseSubscriptionPathMaximumLength(30);

            Assert.AreEqual(30, validationSettings.GetSettings().Get<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.SubscriptionPathMaximumLength));
        }
    }
}