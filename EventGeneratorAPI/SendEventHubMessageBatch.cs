using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text;
using System;
using EventGeneratorAPI.Models;
using EventGeneratorAPI.MessageEngine;
using System.Linq;

namespace EventGeneratorAPI
{
    public static class SendEventHubMessageBatch
    {
        [FunctionName("Job_SendEventHubMessageBatch")]
        public static async Task<string> Run([ActivityTrigger] EventHubJobProperties ehJobProperties, ILogger log)
        {
            try
            {
                EventHubsConnectionStringBuilder connectionStringBuilder = new EventHubsConnectionStringBuilder(ehJobProperties.ConnectionString)
                {
                    EntityPath = ehJobProperties.EventHub
                };

                string connectionString = connectionStringBuilder.ToString();
                EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(connectionString);

                int secondsPerJobBatch = Convert.ToInt16(Environment.GetEnvironmentVariable("secondsPerBatch"));
                int messagesInJobBatch = ehJobProperties.Frequency * secondsPerJobBatch;
                int messagesPerSend = 250;

                IEnumerable<string> messages = Messages.CreateMessages(messagesInJobBatch, ehJobProperties.MessageScheme);

                while (messages.Count() > 0) {
                    var messagesToSend = messages.Take(messagesPerSend);
                    var ehMessages = messagesToSend.Select(m => new EventData(Encoding.UTF8.GetBytes(m)));
                    await eventHubClient.SendAsync(ehMessages);
                    messages = messages.Skip(messagesPerSend);
                }
                await eventHubClient.CloseAsync();

                log.LogInformation($"sent batch of {messagesInJobBatch} messages");
                return $"finished sending {messagesInJobBatch} to {ehJobProperties.EventHub}!";
            }
            catch (Exception exception)
            {
                log.LogInformation($"Exception: {exception.Message}");
                return($"Exception: {exception.Message}");
            }
        }
    }
}
