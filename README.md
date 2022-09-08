# Email Notification Azure Communication Service in Asp.Net Core

## What Azure Service Bus? 
[Azure Service Bus](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview) is a fully managed enterprise message broker with message queues and publish-subscribe topics (in a namespace). Service Bus is used to decouple applications and services from each other

## Queues
Messages are sent to and received from queues. Queues store messages until the receiving application is available to receive and process them.

![11](https://user-images.githubusercontent.com/100709775/186668489-4d3548cb-0498-4b01-85ff-f3c391cc5b71.png)

## Topics
You can also use topics to send and receive messages. While a queue is often used for point-to-point communication, topics are useful in publish/subscribe scenarios.

![22](https://user-images.githubusercontent.com/100709775/186668509-e146ee3b-7908-48bd-b8ce-25d73c972830.png)

Topics can have multiple, independent subscriptions, which attach to the topic and otherwise work exactly like queues from the receiver side.

**In this lab we are focusing only on Queues.** 


## Azure Service Bus trigger for Azure Functions
Azure function with service bus queue trigger is used receive messages from a Service Bus queue or topic. The trigger will be automatically called whenever a new message is queued in a Queue.

## What is Azure Communication Services?

[Azure Communication Services](https://docs.microsoft.com/en-us/azure/communication-services/overview) are cloud-based services with REST APIs and client library SDKs available to help you integrate communication into your applications. You can add communication to your applications without being an expert in underlying technologies such email.

## Email in Azure Communication Services

[Azure Communication Services Email](https://docs.microsoft.com/en-us/azure/communication-services/concepts/email/email-overview) is a new primitive that facilitates high volume transactional, bulk and marketing emails on the Azure Communication Services platform and will enable Application-to-Person (A2P) use cases

# About this exercise

## Backend Code Base:

Previously we have developed an **API** solution in asp.net core in which we have

* EF Code first approach to generate database of a fictitious bank application called **BBBank**.
* We have implemented **AutoWrapper** in BBankAPI project. 
* We have implemented `Deposit` amount feature in **transaction controller**.



## Frontend Codebase
Previously we have angular application in which we have

* FontAwesome library for icons.
* Bootstrap library for styling.
* Created client side models to receive data.
* Created transaction service to call the API.
* Fixed the CORS error on the server side.
* Populated html table, using data returned by API.
* Handled AutoWrapper results.
* We have implement Angular Toaster Notifications.

For more details see [Toaster Notification](https://github.com/PatternsTechGit/PT_ToasterNotification) lab.


## **In this exercise**

**Implementation Architecture**

**Step 1:** Once the amount is deposited and transaction is successfully added in database then we will **add/enqueue** the email related information e.g (To,From,Subject,body) in **Azure Service Bus - Queue**. 

**Step 2:** Once the message is added in Azure Service Bus - Queue then **Azure Function - Service Bus Queue Trigger** will automatically be triggered and **receive/dequeue** the message.

**Step 3:** After receiving the message  we will send it to **Azure Communication Service -Email** which will automatically sends the email on respective email address.


In this exercise again we will be working on **backend** codebase only.

**Backend Codebase**

#### On server side we will:

* Create **Azure Service Bus namespace** & add a new **Queue** in Azure Portal.
* Implement Notification service which will add the emailMessage information in **Azure Service Bus Queue** in code.
* Create **Email Communication Service** in Azure portal.
* Add  **Sub domain** in Communication Service in Azure portal.
* Create **Communication Service** in Azure portal.
* Connect Communication Service with Email domains in Azure portal.
* Create a new **Azure Function** with Service Bus Queue Trigger.
* Implement Azure communicationService to send the email content.

# Configure Azure Service Bus

1. Go to Azure Portal, search Service Bus and create.
   
   ![1](https://user-images.githubusercontent.com/100709775/186673837-847f9b19-2ae8-4313-82fc-1e995f1d6fce.png)

2. Enter the required fields and click `Review + Create`.
   ![2](https://user-images.githubusercontent.com/100709775/186673896-48d697dd-1b21-4069-bfd8-445306539bd4.png)

3. Once the Service Bus is create go to Queues menu and create a new Queue.

![3](https://user-images.githubusercontent.com/100709775/186673925-625d5a78-058a-433a-b9f3-96c21dcd72be.PNG)

4. Go to Shared access policies menu and click **RootManagerSharedAccessKey** and copy the **primary connectionstring**.

![4](https://user-images.githubusercontent.com/100709775/186675089-5e20d1c8-7fd9-4e48-aeb2-4aeffbc9b7b4.PNG)



# Server Side Implementation

Follow the below steps to implement server side code changes:

First we will be installing the `Azure.Messaging.ServiceBus` nuget in **services** project.

```
Install-Package Azure.Messaging.ServiceBus
```

## Step 1: Create AccountByUserResponse class

We will create a new class named **EmailMessage** in **Entities** project  which will contain the email related information  below :

```cs
 public class EmailMessage
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public string[] Recipients { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
    }
```

## Step 2: Creating INotificationService Interface

In **Services** project create an interface (contract) in **Contracts** folder to implement the separation of concerns.
It will make our code testable and injectable as a dependency.

```csharp
 public interface INotificationService
    {
        Task<bool> SendEmail(EmailMessage emailMessage);
    }
```

## Step 3: Implementing NotificationService 

In **Services** project we will create a new class named `NotificationService` for implementing INotificationService interface.

We will inject the `IConfiguration` to get the connectionstring & QueueName from appsettings.

 In `SendEmail` method first we will convert the email message object to JSON and then will add this message in Azure Service Bus Queue. as below   

```csharp
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
            string connectionString = _configuration["ServicebusConnectionString"]; //; "Endpoint=sb://notificationservicebus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=xxxxxxxxxxxxxxxxxxxxxxxxxxxx";
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
```

## Step 4: Dependency Injecting BBBankContext & AccountService 

In `Program.cs` file we will inject the **INotificationService** to services container, so that we can use the relevant object in services.

```cs
builder.Services.AddScoped<INotificationService, NotificationService>();
```

## Step 5: Add ConnectionString & QueueName in AppSettings.Json

Open the AppSettings.Json and add the connectionstring copied from Azure portal and add the queue name.

```json
  "ServicebusConnectionString": "Endpoint=sb://notificationservicebus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
  "QueueName": "emailqueue"
```

## Step 6: SettingUp Transaction Service 

In `TransactionService` we will call the **SendEmail** method of NotificationService.

Go to TransactionService and inject the `INotificationService` using the constructor.

```csharp
 private readonly INotificationService _notificationService;
        
        public TransactionService(BBBankContext BBBankContext, INotificationService NotificationService)
        {
            _bbBankContext = BBBankContext;
            _notificationService = NotificationService;
        }
```

Now we will create an  method **SendEmailNotification** which will call the SendEmail method with EmailMessage as input parameter. 

```csharp
private async void SendEmailNotification(string toAddress, DepositRequest depositRequest)
        {
            EmailMessage emailMessage = new EmailMessage();
            emailMessage.FromAddress = "Notification@PatternsTech.com";
            emailMessage.ToAddress = toAddress;
            emailMessage.Subject = "Amount Deposit Notification";
            emailMessage.Body = depositRequest.Amount+"$ has been deposited successfully.";          
         
            await _notificationService.SendEmail(emailMessage);
        }
```

We will call this method after adding transaction and saveChanges as below :

```cs
public async Task<int> DepositFunds(DepositRequest depositRequest)
        {
            var account = _bbBankContext.Accounts.Where(x => x.AccountNumber == depositRequest.AccountId).FirstOrDefault();
            if (account == null)
                return -1;
            else
            {
                var transaction = new Transaction()
                {
                    Id = Guid.NewGuid().ToString(),
                    TransactionAmount = depositRequest.Amount,
                    TransactionDate = DateTime.UtcNow,
                    TransactionType = TransactionType.Deposit
                };
                if (account.Transactions != null)
                    account.Transactions.Add(transaction);
                
                SendEmailNotification(account.User.Email, depositRequest);
                return 1;
            }
        }
```

**That's it on the BBankAPI project side.** Now we will configure the Azure Communication Email on Azure portal & Azure Function in a new project.

# Configure Azure Communication Service 

## Step 1: SettingUp Email Communication Service 

Go to Azure Portal and search **Email Communication Service** and create it.
Enter the relevant information and click `Review + Create` button. It will create a new Email Communication Service.
![1](https://user-images.githubusercontent.com/100709775/186728974-ce1012e7-7ffa-4f5b-aa23-f632c2b34646.PNG)

Once its created then click on the `1-Click-add` to create a free subdomain, it will automatically create a new domain for our use. 


![2](https://user-images.githubusercontent.com/100709775/186729025-a63d55dd-0566-4cfd-a865-bfc753731fde.PNG)

Once the domain is created, go to **Provision domain** menu and click on the newly created domain. 

![3](https://user-images.githubusercontent.com/100709775/186729096-804b60ff-2f51-4075-9237-4dee14b25faa.PNG)

Copy the **MailFrom** information it will be used when we send the message to Azure communication service in Asp.Net Core code.

![4](https://user-images.githubusercontent.com/100709775/186729154-4fac4fc6-f9b5-41bd-b8f1-ab005051312a.PNG)

## Step 2: SettingUp  Communication Service 

Search **Communication Service** and create it.
Enter the relevant information and click `Review + Create` button. It will create a new Communication Service.

![5](https://user-images.githubusercontent.com/100709775/186729217-efd0b39f-45ec-4a84-ac4d-414dc43d883e.PNG)

Click on the `Connect your email domains` button to connect the communication service with email service.

![6](https://user-images.githubusercontent.com/100709775/186729259-1ad1acaf-bd5d-4845-a332-f503d86c7a81.PNG)

Select the relevant information such as subscription,Email Service and click `Connect` button.

![7](https://user-images.githubusercontent.com/100709775/186729378-ab8d64e3-b026-4496-bc5f-79daa65a51d7.PNG)

Go to **Keys** menu and copy the **connection string** as below

![8](https://user-images.githubusercontent.com/100709775/186729432-e7b2c8be-9598-4b76-ab12-964aaa53e235.PNG)

# Server Side Implementation

## Step 1: Create Azure Function Project

Open visual studio and create a new project, select **Azure Functions** and click next.

![1](https://user-images.githubusercontent.com/100709775/187843832-2bd76665-fdcd-4129-94d3-0a4ec5693a9d.PNG)

Enter the project name and then click next. 

Select the Function type **Service Bus Queue trigger**, enter the connectionstring **key** name and queue name, click **Create** button.

![2](https://user-images.githubusercontent.com/100709775/187849769-5981ab7b-8d2f-4960-8262-102e8dcf3974.PNG)



## Step 2: Install Nuget package

First we will be installing the `Azure.Communication.Email` nuget in project.

Run the commands below to install the nuget. 
```
Install-Package Azure.Communication.Email -Version 1.0.0-beta.1

Install-Package Newtonsoft.Json
```

## Step 3: Setup AzureCommunicationService

Create a new class named `EmailMessage` which will contains the email related information as below 

```cs
public class EmailMessage
{
    public string Subject { get; set; }
    public string Body { get; set; }
    public string[] Recipients { get; set; }
    public string FromAddress { get; set; }
    public string ToAddress { get; set; }
}
```

Create a new class named `IAzureCommunicationService` which will contain a method named **SendEmail** as below : 

```cs
public interface IAzureCommunicationService
    {
        bool SendEmail(EmailMessage emailMessage);
    }
```

Create a new class named `AzureCommunicationService` which will implement the interface `IAzureCommunicationService` and send the message to Azure Communication service.

```cs
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
```

## Configure local.settings.json

Open the `local.settings.json` file, paste the Azure service bus connectionstring value against key **ConnectionString**. paste Azure Communication service connection string against key **AzureCommunicationConnectionString** and lastly paste the sender information against ket **sender**.

```json
{
    "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "ConnectionString": "Endpoint=sb://notificationservicebus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=XXXXXXXXXXXXXXXXXXXXXXXX",
    "AzureCommunicationConnectionString": "endpoint=https://notificationcommunicationservice.communication.azure.com/;accesskey=jqmVRmGz2aRnW4AK4p8WDX/XXXXXXXXXXXXXXXXXXXXXXXX,
    "sender": "DoNotReply@b38b462a-e178-xxxx-912e-cb2d9334e9b4.azurecomm.net"
  }
}
```

## Configure Startup

Crete a new class named `Startup.cs` and configure register the `IAzureCommunicationService  as below 

```cs
// marking this file as a startup file
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
```

Renamed the `Function1.cs` class to `ServiceBusTrigger.cs` and function name to **ServiceBusQueueTrigger**. 

Dependency inject the `IAzureCommunicationService`. Enter the service bus **queue name** in first parameter and enter the service bus connection string **key name** ( value already pasted in local.settings.json) in second parameter of **ServiceBusTrigger**. 

Once the function is triggered automatically we will **Deserialize** the json string to **EmailMessage** object and call the **SendEmail** method of 
CommunicationService as below.
```cs
public class ServiceBusTrigger
    {
        //Dependency Injecting IAzureCommunicationService.
        IAzureCommunicationService _communicationService;
        public ServiceBusTrigger(IAzureCommunicationService communicationService)
        {
            _communicationService = communicationService;
        }

        [FunctionName("ServiceBusQueueTrigger")]
        public void Run([ServiceBusTrigger("emailqueue", Connection = "connectionString")] string myQueueItem, ILogger log)
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
```

## Final  Output

Run the BBankAPI project then run the Azure function project. 

Run the BBankUI project, enter any amount and click deposit button. You may place breakpoint in **ServiceBusTrigger** class in azure function project and while sending email at this line `_communicationService.SendEmail(emailMessage);` put your email in **emailMessage** object's `ToAddress` property and then you should receive an email for deposited amount.


![DepositFunds](https://user-images.githubusercontent.com/100709775/185105908-ce8991b3-237e-4af7-80d6-7f241a84566f.gif)
