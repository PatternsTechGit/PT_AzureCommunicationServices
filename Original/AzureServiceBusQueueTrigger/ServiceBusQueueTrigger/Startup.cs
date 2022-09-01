
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceBusQueueTrigger;
// marking this file as a startup file
[assembly: FunctionsStartup(typeof(BBBankFunctions.Startup))]
namespace BBBankFunctions
{
    //FunctionsStartup is part of Dependency Injection  Nuget
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            //Dependency Injecting IAzureCommunicationService
            builder.Services.AddScoped<IAzureCommunicationService, AzureCommunicationService>();
        }
    }
}
