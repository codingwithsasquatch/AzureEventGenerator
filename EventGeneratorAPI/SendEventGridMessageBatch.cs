using System.Threading.Tasks;
using Microsoft.Rest;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
//using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using System.Text;
using System;
using EventGeneratorAPI.Models;
using EventGeneratorAPI.MessageEngine;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;


namespace EventGeneratorAPI
{
    public static class SendEventGridMessageBatch
    {

        private static readonly HttpClient HttpClient;

        static SendEventGridMessageBatch()
        {
            HttpClient = new HttpClient();
        }

        [FunctionName("Job_SendEventGridMessageBatch")]
        public static async Task<string> Run([ActivityTrigger] EventGridJobProperties egJobProperties, TraceWriter log)
        {
            //ServiceClientCredentials credentials = new TopicCredentials(egJobProperties.Key);
            //EventGridClient client = new EventGridClient(credentials);
            
            int secondsPerBatch = Convert.ToInt16(Environment.GetEnvironmentVariable("secondsPerBatch"));
            int messagesInBatch = egJobProperties.Frequency * secondsPerBatch;
            IEnumerable<string> messages = Messages.CreateMessages(messagesInBatch, egJobProperties.MessageScheme);

            try
            {

                HttpClient.DefaultRequestHeaders.Add("aeg-sas-key", egJobProperties.Key);
                HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("AzureEventGenerator");

                var egMessages = messages.Select(m => new EventGridEvent()
                    {
                        Subject = egJobProperties.MessageScheme.ToLower(),
                        EventType = egJobProperties.MessageScheme.ToLower(),
                        EventTime = DateTime.UtcNow,
                        Id = Guid.NewGuid().ToString(),
                        DataVersion = "1.0",
                        Data = m
                    }
                ).ToList();

                //await client.PublishEventsAsync(egJobProperties.Endpoint, egMessages);
                string json = JsonConvert.SerializeObject(egMessages);

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, egJobProperties.Endpoint)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                HttpResponseMessage response = await HttpClient.SendAsync(request);
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
