
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;

namespace AzureServiceBusQueueSend
{
    class Program
    {
        static IQueueClient client;
        static async Task Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();
          
            var connectionString = config["connectionString"];
            client = new QueueClient(connectionString, "demoqueue4261");  

            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };

            client.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
            Console.WriteLine("Waiting for a message.  Type q to quit.");
            Console.ReadKey();
            await client.CloseAsync();
        }

        static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }

        static async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            Console.WriteLine($"Received message: SequenceNumber: {message.SystemProperties.SequenceNumber} \nBody: {Encoding.UTF8.GetString(message.Body)}");
            await client.CompleteAsync(message.SystemProperties.LockToken);
        }
    }
}