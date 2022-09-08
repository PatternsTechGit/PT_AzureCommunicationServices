using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureServiceBusQueueTrigger
{
    public class ServiceBusTrigger
    {
        IAzureCommunicationService _communicationService;
        public ServiceBusTrigger(IAzureCommunicationService communicationService)
        {
            _communicationService = communicationService;
        }

        [FunctionName("ServiceBusQueueTrigger")]
        public void Run([ServiceBusTrigger("testemailqueue", Connection = "ConnectionString")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");

            //Deserialize the json string to EmailMessage object.
            EmailMessage emailMessage = JsonConvert.DeserializeObject<EmailMessage>(myQueueItem);
            if (emailMessage != null)
            {
                _communicationService.SendEmail(emailMessage);
            }
        }
    }
}
