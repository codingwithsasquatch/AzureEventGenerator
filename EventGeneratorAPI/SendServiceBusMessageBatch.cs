using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.ServiceBus;
using System.Collections.Generic;
using System.Text;
using System;
using EventGeneratorAPI.Models;
using EventGeneratorAPI.MessageEngine;
using System.Linq;

namespace EventGeneratorAPI
{
    public static class SendServiceBusMessageBatch
    {
        [FunctionName("Job_SendServiceBusMessageBatch")]
        public static async Task<string> Run([ActivityTrigger] ServiceBusJobProperties sbJobProperties, TraceWriter log)
        {
            QueueClient queueClient = new QueueClient(sbJobProperties.ConnectionString, sbJobProperties.Queue);

            int secondsPerBatch = Convert.ToInt16(Environment.GetEnvironmentVariable("secondsPerBatch"));
            int messagesInBatch = sbJobProperties.Frequency * secondsPerBatch;
            IEnumerable<string> messages = Messages.CreateMessages(messagesInBatch, sbJobProperties.MessageScheme);

            try
            {
                var sbMessages = (List<Message>) messages.Select(m => new Message() 
                    {
                        Body = Encoding.UTF8.GetBytes(m),
                        ContentType = "text/plain" 
                    }
                );

                await queueClient.SendAsync(sbMessages);
                await queueClient.CloseAsync();
            }
            catch (Exception exception)
            {
                log.Info($"Exception: {exception.Message}");
            }

            log.Info($"sent batch of {messages.Count()} messages");
            return $"finished sending {messages.Count()} to {sbJobProperties.Queue}!";
        }
    }
}
