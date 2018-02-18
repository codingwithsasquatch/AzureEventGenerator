using System.Threading.Tasks;
using Microsoft.Rest;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using System.Text;
using System;
using EventGeneratorAPI.Models;
using EventGeneratorAPI.MessageEngine;
using System.Collections.Generic;
using System.Linq;

namespace EventGeneratorAPI
{
    public static class SendEventGridMessageBatch
    {
        [FunctionName("Job_SendEventGridMessageBatch")]
        public static async Task<string> Run([ActivityTrigger] EventGridJobProperties egJobProperties, TraceWriter log)
        {
            ServiceClientCredentials credentials = new TopicCredentials(egJobProperties.Key);
            EventGridClient client = new EventGridClient(credentials);
            
            int secondsPerBatch = Convert.ToInt16(Environment.GetEnvironmentVariable("secondsPerBatch"));
            int messagesInBatch = egJobProperties.Frequency * secondsPerBatch;
            IEnumerable<string> messages = Messages.CreateMessages(messagesInBatch, egJobProperties.MessageScheme);

            try
            {
                var egMessages = (List<EventGridEvent>) messages.Select(m => new EventGridEvent()
                    {
                        Subject = egJobProperties.MessageScheme.ToLower(),
                        EventType = egJobProperties.MessageScheme.ToLower(),
                        EventTime = DateTime.UtcNow,
                        Id = Guid.NewGuid().ToString(),
                        Data = m
                    }
                );

                await client.PublishEventsAsync(egJobProperties.Endpoint, egMessages);
            }
            catch (Exception exception)
            {
                log.Info($"Exception: {exception.Message}");
            }

            log.Info($"sent batch of {messages.Count()} messages");
            return $"finished sending {messages.Count()} to {egJobProperties.Endpoint}!";
        }
    }
}
