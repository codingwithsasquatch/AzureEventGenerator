
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using EventGeneratorAPI.Models;
using EventGeneratorAPI.MessageEngine;

namespace EventGeneratorAPI
{
    public static class StorageQueueMessageGenerator
    {
        [FunctionName("Job_StorageQueueMessageGenerator")]
        public static async Task<string> Run([ActivityTrigger] StorageQueueJobProperties sqJobProperties, TraceWriter log)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(sqJobProperties.ConnectionString);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference(sqJobProperties.Queue);
            await queue.CreateIfNotExistsAsync();

            int numOfMessages = sqJobProperties.Frequency * 60 * sqJobProperties.Duration;
            string[] messages = Messages.CreateMessages(numOfMessages, sqJobProperties.MessageScheme);

            for (var i = 0; i < numOfMessages; i++)
            {
                try
                {
                    var message = messages[i];
                    log.Info($"Sending message: {message}");
                    CloudQueueMessage cloudQueueMessage = new CloudQueueMessage(message);
                    await queue.AddMessageAsync(cloudQueueMessage);
                }
                catch (Exception exception)
                {
                    log.Info($"Exception: {exception.Message}");
                }
                await Task.Delay(1000 / sqJobProperties.Frequency);
            }

            log.Info($"finished.");
            return $"finished sending {numOfMessages} to {sqJobProperties.Queue}!";
        }
    }
}
