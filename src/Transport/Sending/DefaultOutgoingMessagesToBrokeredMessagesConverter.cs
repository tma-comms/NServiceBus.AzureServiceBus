namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.DelayedDelivery;
    using NServiceBus.Settings;
    using NServiceBus.Transports;

    public class DefaultOutgoingMessagesToBrokeredMessagesConverter : IConvertOutgoingMessagesToBrokeredMessages
    {
        readonly ReadOnlySettings settings;

        public DefaultOutgoingMessagesToBrokeredMessagesConverter(ReadOnlySettings settings)
        {
            this.settings = settings;
        }

        public IEnumerable<BrokeredMessage> Convert(IEnumerable<OutgoingMessage> outgoingMessages, RoutingOptions routingOptions)
        {
            return outgoingMessages.Select(message => Convert(message, routingOptions));
        }

        public BrokeredMessage Convert(OutgoingMessage outgoingMessage, RoutingOptions routingOptions)
        {
            var brokeredMessage = CreateBrokeredMessage(outgoingMessage);
            brokeredMessage.MessageId = outgoingMessage.MessageId;

            CopyHeaders(outgoingMessage, brokeredMessage);

            ApplyDeliveryConstraints(brokeredMessage, routingOptions.DispatchOptions);

            ApplyTimeToLive(outgoingMessage, brokeredMessage);

            ApplyCorrelationId(outgoingMessage, brokeredMessage);

            SetReplyToAddress(outgoingMessage, brokeredMessage);

            SetViaPartitionKeyToIncomingBrokeredMessagePartitionKey(brokeredMessage, routingOptions);

            return brokeredMessage;
        }

        private void SetViaPartitionKeyToIncomingBrokeredMessagePartitionKey(BrokeredMessage brokeredMessage, RoutingOptions routingOptions)
        {
            if (routingOptions.SendVia && routingOptions.ViaPartitionKey != null)
            {
                brokeredMessage.ViaPartitionKey = routingOptions.ViaPartitionKey;
            }
        }

        void SetReplyToAddress(OutgoingMessage outgoingMessage, BrokeredMessage brokeredMessage)
        {
            if (outgoingMessage.Headers.ContainsKey(Headers.ReplyToAddress))
            {
                brokeredMessage.ReplyTo = outgoingMessage.Headers[Headers.ReplyToAddress];
            }
        }

        void ApplyCorrelationId(OutgoingMessage outgoingMessage, BrokeredMessage brokeredMessage)
        {
            if (outgoingMessage.Headers.ContainsKey(Headers.CorrelationId))
            {
                brokeredMessage.CorrelationId = outgoingMessage.Headers[Headers.CorrelationId];
            }
        }

        void ApplyTimeToLive(OutgoingMessage outgoingMessage, BrokeredMessage brokeredMessage)
        {
            TimeSpan? timeToLive = null;
            if (outgoingMessage.Headers.ContainsKey(Headers.TimeToBeReceived))
            {
                TimeSpan ttl;
                TimeSpan.TryParse(outgoingMessage.Headers[Headers.TimeToBeReceived], out ttl);
                timeToLive = ttl;
            }

            if (timeToLive.HasValue && timeToLive.Value > TimeSpan.Zero)
            {
                brokeredMessage.TimeToLive = timeToLive.Value;
            }
        }

        void ApplyDeliveryConstraints(BrokeredMessage brokeredMessage, DispatchOptions dispatchOptions)
        {
            DateTime? scheduledEnqueueTime = null;

            var deliveryConstraint = dispatchOptions.DeliveryConstraints.FirstOrDefault(d => d is DelayedDeliveryConstraint);

            if (deliveryConstraint != null)
            {
                var delay = deliveryConstraint as DelayDeliveryWith;
                if (delay != null)
                {
                    scheduledEnqueueTime = Time.UtcNow() + delay.Delay;
                }
                else
                {
                    var exact = deliveryConstraint as DoNotDeliverBefore;
                    if (exact != null)
                    {
                        scheduledEnqueueTime = exact.At;
                    }
                }
            }

            if (scheduledEnqueueTime.HasValue)
                   brokeredMessage.ScheduledEnqueueTimeUtc = scheduledEnqueueTime.Value;
        }

        static void CopyHeaders(OutgoingMessage outgoingMessage, BrokeredMessage brokeredMessage)
        {
            foreach (var header in outgoingMessage.Headers)
            {
                brokeredMessage.Properties[header.Key] = header.Value;
            }
        }

        BrokeredMessage CreateBrokeredMessage(OutgoingMessage outgoingMessage)
        {
            BrokeredMessage brokeredMessage;
            var bodyType = settings.Get<SupportedBrokeredMessageBodyTypes>(WellKnownConfigurationKeys.Serialization.BrokeredMessageBodyType);
            switch (bodyType)
            {
                case SupportedBrokeredMessageBodyTypes.ByteArray:
                    brokeredMessage = outgoingMessage.Body != null ? new BrokeredMessage(outgoingMessage.Body) : new BrokeredMessage();
                    break;
                case SupportedBrokeredMessageBodyTypes.Stream:
                    brokeredMessage = outgoingMessage.Body != null ? new BrokeredMessage(new MemoryStream(outgoingMessage.Body)) : new BrokeredMessage();
                    break;
                default:
                    throw new ConfigurationErrorsException("Unsupported brokered message body type configured");
            }
            return brokeredMessage;
        }
    }

    static class Time
    {
        public static Func<DateTime> UtcNow = () => DateTime.UtcNow;
    }
}