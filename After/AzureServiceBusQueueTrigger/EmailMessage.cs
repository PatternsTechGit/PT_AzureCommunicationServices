using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureServiceBusQueueTrigger
{
    public class EmailMessage
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public string[] Recipients { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
    }
}
