
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventHubs;
using System.Text;
using System;
using EventGeneratorAPI.Models;
using EventGeneratorAPI.MessageEngine;
using System.Collections.Generic;
using System.Linq;

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

            string connectionString = connectionStringBuilder.ToString();
            int numOfMessages = ehJobProperties.Frequency * 60 * ehJobProperties.Duration;
            IEnumerable<string> messages = new List<string>(Messages.CreateMessages(numOfMessages, ehJobProperties.MessageScheme));
            //IList<string> msgs = new List<string>(messages);

            int secondsPerBatch = 5;
            int messagesPerBatch = secondsPerBatch * ehJobProperties.Frequency;

            while (messages.Count() >0)
            {
                await SendMessageBatch( messages.Take(messagesPerBatch), connectionString, log);
                messages = messages.Skip(messagesPerBatch);
                log.Info($"sent batch of {numOfMessages} messages");
            }

            log.Info($"finished.");
            return $"finished sending {numOfMessages} to {ehJobProperties.EventHub}!";
        }

        public static async Task SendMessageBatch(IEnumerable<string> messages, string connectionString, TraceWriter log)
        {
            try
            {
                EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(connectionString);
                var ehMessages = messages.Select(m => new EventData(Encoding.UTF8.GetBytes(m)));

                await eventHubClient.SendAsync(ehMessages);
                await eventHubClient.CloseAsync();
            }
            catch (Exception exception)
            {
                log.Info($"Exception: {exception.Message}");
            }
        }
    }
}
