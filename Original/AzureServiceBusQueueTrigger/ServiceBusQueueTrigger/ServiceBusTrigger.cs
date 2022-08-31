using System;
using System.Collections.Generic;
using System.Net.Mail;
using Azure.Communication.Email;
using Azure.Communication.Email.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ServiceBusQueueTrigger
{
    public class ServiceBusTrigger
    {
        IAzureCommunicationService _communicationService;
        public ServiceBusTrigger(IAzureCommunicationService communicationService)
        {
            _communicationService = communicationService;
        }


        [FunctionName("ServiceBusQueueTrigger")]
        public void Run([ServiceBusTrigger("emailqueue", Connection = "connectionString")] string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");

            EmailMessage emailMessage = JsonConvert.DeserializeObject<EmailMessage>(myQueueItem);
            if (emailMessage != null)
            {
                _communicationService.SendEmail(emailMessage);
            }
        }


    }
}
