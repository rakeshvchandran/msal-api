using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace msal_api
{
    public class ServiceBusMessageHandler : IServiceBusMessageHandler
    {
        private readonly IConfiguration _configuration;
        public ServiceBusMessageHandler(IConfiguration configuration)
        {
            _configuration=configuration;
        }

        public async Task SendMessageAsync(FileInfoMessage fileInfoMessage)
        {
            var connectionString = _configuration.GetValue<string>("AzureServiceBusConnectionString");
            var queueName = _configuration.GetValue<string>("AzureServiceBusQueue");
            // since ServiceBusClient implements IAsyncDisposable we create it with "await using"
            await using var client = new ServiceBusClient(connectionString);

            var jsonMessage = JsonSerializer.Serialize(fileInfoMessage);
            // create the sender
            ServiceBusSender sender = client.CreateSender(queueName);

            var message = new ServiceBusMessage(jsonMessage);


            // send the message
            await sender.SendMessageAsync(message);
        }
    }
}
