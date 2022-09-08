using Azure.Communication.Email.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureServiceBusQueueTrigger
{
    public interface IAzureCommunicationService
    {
        bool SendEmail(EmailMessage emailMessage);
    }
}
