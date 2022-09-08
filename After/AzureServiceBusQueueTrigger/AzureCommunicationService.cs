using Azure.Communication.Email.Models;
using Azure.Communication.Email;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureServiceBusQueueTrigger
{
    public class AzureCommunicationService : IAzureCommunicationService
    {
        IConfiguration _configuration;
        public AzureCommunicationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool SendEmail(EmailMessage emailMessageDto)
        {
            var connectionString = _configuration["AzureCommunicationConnectionString"]; // Find your Communication Services resource in the Azure portal
            EmailClient client = new EmailClient(connectionString);
            // Create the email content
            var emailContent = new EmailContent(emailMessageDto.Subject);
            emailContent.PlainText = emailMessageDto.Body;

            // Create the recipient list
            var emailRecipients = new EmailRecipients(
                new List<EmailAddress>
                {
        new EmailAddress(
            email: emailMessageDto.ToAddress )
                });

            // Create the EmailMessage
            var emailMessage = new Azure.Communication.Email.Models.EmailMessage(
                sender: _configuration["sender"].ToString(),
            emailContent,
                emailRecipients);
            //Send message to Azure Communication Service 
            SendEmailResult sendResult = client.Send(emailMessage);

            //Check status of newly send message 
            SendStatusResult status = client.GetSendStatus(sendResult.MessageId);
            return true;
        }
    }
}
