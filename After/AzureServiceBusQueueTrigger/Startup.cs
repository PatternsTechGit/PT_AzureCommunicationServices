using AzureServiceBusQueueTrigger;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(BBBankFunctions.Startup))]
namespace BBBankFunctions
{
    //FunctionsStartup is part of Dependency Injection  Nuget
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            //Dependency Injecting IAzureCommunicationService.
            builder.Services.AddScoped<IAzureCommunicationService, AzureCommunicationService>();
        }
    }
}