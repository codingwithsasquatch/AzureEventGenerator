using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System.Text;
using System;
using Microsoft.Azure.ServiceBus;
using EventGeneratorAPI.Models;
using EventGeneratorAPI.MessageEngine;

namespace EventGeneratorAPI
{
    public static class ServiceBusMessageGenerator
    {
        [FunctionName("Job_ServiceBusMessageGenerator")]
        public static async Task<string> Run([ActivityTrigger] ServiceBusJobProperties sbJobProperties, TraceWriter log)
        {
            QueueClient queueClient = new QueueClient(sbJobProperties.ConnectionString, sbJobProperties.Queue);

            int numOfMessages = sbJobProperties.Frequency * 60 * sbJobProperties.Duration;
            string[] messages = Messages.CreateMessages(numOfMessages, sbJobProperties.MessageScheme);
            for (var i = 0; i < numOfMessages; i++)
            {
                try
                {
                    var message = messages[i]; //CreateMessage(i, sbJobProperties.messageScheme);
                    log.Info($"Sending message: {message}");
                    var body = Encoding.UTF8.GetBytes(message);
                    await queueClient.SendAsync(new Message { Body = body, ContentType = "text/plain" });
                }
                catch (Exception exception)
                {
                    log.Info($"Exception: {exception.Message}");
                }
                await Task.Delay(1000 / sbJobProperties.Frequency);
            }
            await queueClient.CloseAsync();

            log.Info($"finished.");
            return $"finished sending {numOfMessages} to {sbJobProperties.Queue}!";
        }
    }
}
