
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Mango.MessageBus
{
    public class AzureServiceBusMessageBus : IMessageBus
    {
        //connection of azure service bus topic subscribe shared access  conn 
        private string connectionString = "Endpoint=sb://mangorestaurant.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=7t3usJ6tooj30PRi5ccrTwbxS6DNGPxbmp2oVdiO3cI=";


        /// <summary>
        /// Publissh message to azure bus
        /// </summary>
        /// <param name="message">message to publish</param>
        /// <param name="topicName">Service Bus sujet/topic bus name</param>
        /// <returns></returns>
        /// <remarks>
        /// use Azure.Messaging.ServiceBus;
        /// </remarks>
        public async Task PublishMessage(BaseMessage message, string topicName)
        {

            //Connect to Service-BUS
            await using var client = new ServiceBusClient(connectionString);
            //create bus sender message for topic
            ServiceBusSender sender = client.CreateSender(topicName);
            //Create message to sender in bus
            var jsonMessage = JsonConvert.SerializeObject(message);
           
            //Create Bus message to send
            ServiceBusMessage finalMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(jsonMessage))
            {
                CorrelationId = Guid.NewGuid().ToString()
            };

            //Send bus message
            await sender.SendMessageAsync(finalMessage);

            //Close service bus
            await client.DisposeAsync();
        }
    }
}
