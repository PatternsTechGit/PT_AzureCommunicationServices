using Azure.Messaging.ServiceBus;
using Entities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class NotificationService : INotificationService
    {
        private readonly IConfiguration _configuration;
        public NotificationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<bool> SendEmail(EmailMessage emailMessage)
        {
            var jsonMessage = JsonConvert.SerializeObject(emailMessage);

            // the client that owns the connection and can be used to create senders and receivers
            string connectionString = _configuration["ServicebusConnectionString"]; //; "Endpoint=sb://notificationservicebus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=uFjmz0OP+dcSA9AVZJ+nHQQCYN+MH6lmCn4Dh1yBZy0=";
            string queueName = _configuration["QueueName"];//"emailqueue";

            // the sender used to publish messages to the queue

            // The Service Bus client types are safe to cache and use as a singleton for the lifetime
            // of the application, which is best practice when messages are being published or read
            // regularly.
            //
            // Create the clients that we'll use for sending and processing messages.
            ServiceBusClient client = new ServiceBusClient(connectionString);
            ServiceBusSender sender = client.CreateSender(queueName);

            try
            {
                // Use the producer client to send the message to the Service Bus queue
                await sender.SendMessageAsync(new ServiceBusMessage(jsonMessage));
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other un-managed objects are properly cleaned up.
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }
            return true;
        }
    }
}
