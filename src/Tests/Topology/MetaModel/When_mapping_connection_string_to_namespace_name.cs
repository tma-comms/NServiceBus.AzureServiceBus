﻿namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology.MetaModel
{
    using System.Collections.Generic;
    using AzureServiceBus;
    using AzureServiceBus.Topology.MetaModel;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_mapping_connection_string_to_namespace_name
    {
        DefaultConnectionStringToNamespaceNameMapper mapper;

        [SetUp]
        public void SetUp()
        {
            var namespaceConfigurations = new NamespaceConfigurations();
            namespaceConfigurations.Add("namespace1", "Endpoint=sb://namespace1.servicebus.windows.net;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=secret");
            namespaceConfigurations.Add("namespace2", "Endpoint=sb://namespace2.servicebus.windows.net;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=secret");
            namespaceConfigurations.Add("namespace3", "Endpoint=sb://namespace3.servicebus.windows.net;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=secret");
            var settings = new SettingsHolder();
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, namespaceConfigurations);

            mapper = new DefaultConnectionStringToNamespaceNameMapper(settings);
        }

        [Test]
        [TestCase("queuename")]
        [TestCase("queuename@notAConnectionString")]
        public void Should_return_same_value_if_does_not_contain_connection_string(string value)
        {
            var mappedValue = mapper.Map(value);

            StringAssert.AreEqualIgnoringCase(value, mappedValue);
        }

        [Test]
        public void Should_throw_if_connection_string_has_not_been_mapped()
        {
            Assert.Throws<KeyNotFoundException>(() => mapper.Map("queuename@Endpoint=sb://namespace.servicebus.windows.net;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=secret"));
        }

        [Test]
        public void Should_return_mapped_value_with_right_namespace_name()
        {
            var mappedValue = mapper.Map("queuename@Endpoint=sb://namespace1.servicebus.windows.net;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=secret");

            StringAssert.AreEqualIgnoringCase("queuename@namespace1", mappedValue);
        }
    }
}
