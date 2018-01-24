
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventHubs;
using System.Text;
using System;
using EventGeneratorAPI.Models;
using EventGeneratorAPI.MessageEngine;

namespace EventGeneratorAPI
{
    public static class EventHubMessageGenerator
    {
        [FunctionName("Job_EventHubMessageGenerator")]
        public static async Task<string> Run([ActivityTrigger] EventHubJobProperties ehJobProperties, TraceWriter log)
        {
            EventHubsConnectionStringBuilder connectionStringBuilder = new EventHubsConnectionStringBuilder(ehJobProperties.ConnectionString)
            {
                EntityPath = ehJobProperties.EventHub
            };

            int numOfMessages = ehJobProperties.Frequency * 60 * ehJobProperties.Duration;

            try
            {
                EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());

                string[] messages = Messages.CreateMessages(numOfMessages, ehJobProperties.MessageScheme);
                for (var i = 0; i < numOfMessages; i++)
                {
                    var message = messages[i];
                    log.Info($"Sending message: {message}");
                    await eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(message)));
                    await Task.Delay(1000 / ehJobProperties.Frequency);
                }

                await eventHubClient.CloseAsync();
            }
            catch (Exception exception)
            {
                log.Info($"Exception: {exception.Message}");
                return $"Exception: {exception.Message}";
            }

            log.Info($"finished.");
            return $"finished sending {numOfMessages} to {ehJobProperties.EventHub}!";
        }
    }
}
