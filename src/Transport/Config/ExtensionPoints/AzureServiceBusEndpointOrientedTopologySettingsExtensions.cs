﻿namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using AzureServiceBus;
    using AzureServiceBus.TypesScanner;
    using Configuration.AdvanceExtensibility;
    using Settings;

    public static class AzureServiceBusEndpointOrientedTopologySettingsExtensions
    {
        public static AzureServiceBusTopologySettings<EndpointOrientedTopology> RegisterPublisherForType(this AzureServiceBusTopologySettings<EndpointOrientedTopology> topologySettings, string publisherName, Type type)
        {
            AddScannerForPublisher(topologySettings.GetSettings(), publisherName, new SingleTypeScanner(type));
            return topologySettings;
        }

        public static AzureServiceBusTopologySettings<EndpointOrientedTopology> RegisterPublisherForAssembly(this AzureServiceBusTopologySettings<EndpointOrientedTopology> topologySettings, string publisherName, Assembly assembly)
        {
            AddScannerForPublisher(topologySettings.GetSettings(), publisherName, new AssemblyTypesScanner(assembly));
            return topologySettings;
        }

        static void AddScannerForPublisher(SettingsHolder settings, string publisherName, ITypesScanner scanner)
        {
            Dictionary<string, List<ITypesScanner>> map;

            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Publishers, out map))
            {
                map = new Dictionary<string, List<ITypesScanner>>();
                settings.Set(WellKnownConfigurationKeys.Topology.Publishers, map);
            }

            if (!map.ContainsKey(publisherName))
                map[publisherName] = new List<ITypesScanner>();

            map[publisherName].Add(scanner);
        }
    }
}