using Azure.Communication.Email.Models;
using Azure.Communication.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ServiceBusQueueTrigger
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


            var connectionString = _configuration["AzureCommunicationConnectionString"]; // ; "endpoint=https://notificationcommunicationservice.communication.azure.com/;accesskey=jqmVRmGz2aRnW4AK4p8WDX/3bevgL+JOgFws47e2MBwxoagyUHefWGpE4szgnF1H6c2GB/ubzyVOv3tPpoeY9w=="; // Find your Communication Services resource in the Azure portal
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
                sender: _configuration["sender"].ToString(), //"DoNotReply@b38b462a-e178-4e09-912e-cb2d9334e9b4.azurecomm.net",// The email address of the domain registered with the Communication Services resource
            emailContent,
                emailRecipients);

            SendEmailResult sendResult = client.Send(emailMessage);
            
            SendStatusResult status = client.GetSendStatus(sendResult.MessageId);
            return true;

        }
    }
}
