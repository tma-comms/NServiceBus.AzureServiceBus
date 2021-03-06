namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

    class MessageReceiverAdapter : IMessageReceiver
    {
        MessageReceiver receiver;

        public MessageReceiverAdapter(MessageReceiver receiver)
        {
            this.receiver = receiver;
        }

        public bool IsClosed => receiver.IsClosed;

        public RetryPolicy RetryPolicy
        {
            get { return receiver.RetryPolicy; }
            set { receiver.RetryPolicy = value; }
        }

        public int PrefetchCount
        {
            get { return receiver.PrefetchCount; }
            set { receiver.PrefetchCount = value; }
        }

        public ReceiveMode Mode => receiver.Mode;

        public void OnMessage(Func<BrokeredMessage, Task> callback, OnMessageOptions options)
        {
            receiver.OnMessageAsync(callback, options);
        }

        public Task CloseAsync()
        {
           return receiver.CloseAsync();
        }
    }
}