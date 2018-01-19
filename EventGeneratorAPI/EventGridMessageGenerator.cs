using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Net.Http.Headers;
using EventGeneratorAPI.Models;
using EventGeneratorAPI.MessageEngine;

namespace EventGeneratorAPI
{
    public static class EventGridMessageGenerator
    {
        [FunctionName("Job_EventGridMessageGenerator")]
        public static async Task<string> Run([ActivityTrigger] EventGridJobProperties egJobProperties, TraceWriter log)
        {
            var client = new HttpClient { BaseAddress = new Uri(egJobProperties.Endpoint) };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("aeg-sas-key", egJobProperties.Key);

            int numOfMessages = egJobProperties.Frequency * 60 * egJobProperties.Duration;
            string[] messages = Messages.CreateMessages(numOfMessages, egJobProperties.MessageScheme);
            for (var i = 0; i < numOfMessages; i++)
            {
                try
                {
                    var message = messages[i];
                    log.Info($"Sending message {i}: {message}");
                    var data = new[]
                    {
                        new
                        {
                            Subject = egJobProperties.MessageScheme.ToLower(),
                            EventType = egJobProperties.MessageScheme.ToLower(),
                            EventTime = DateTime.UtcNow,
                            Id = Guid.NewGuid().ToString(),
                            Data = message
                        }
                    };
                    var response = await client.PostAsJsonAsync(string.Empty, data);
                }
                catch (Exception exception)
                {
                    log.Info($"Exception: {exception.Message}");
                }
                await Task.Delay(1000 / egJobProperties.Frequency);
            }

            log.Info($"finished.");
            return $"finished sending {numOfMessages} to {egJobProperties.Endpoint}!";
        }
    }
}
