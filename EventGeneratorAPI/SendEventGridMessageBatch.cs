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
        [FunctionName("Job_EventHubMessageGenerator")]
        public static async Task<string> Run([ActivityTrigger] EventGridJobProperties egJobProperties, TraceWriter log)
        {
            ServiceClientCredentials credentials = new TopicCredentials(egJobProperties.Key);
            var client = new EventGridClient(credentials);

            try
            {
                var egMessages = (List<EventGridEvent>) egJobProperties.Messages.Select(m => new EventGridEvent()
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

            log.Info($"sent batch of {egJobProperties.Messages.Count()} messages");
            return $"finished sending {egJobProperties.Messages.Count()} to {egJobProperties.Endpoint}!";
        }
    }
}
