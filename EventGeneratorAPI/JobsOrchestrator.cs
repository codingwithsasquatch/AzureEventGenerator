
using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using EventGeneratorAPI.Models;
using EventGeneratorAPI.MessageEngine;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace EventGeneratorAPI
{
    public static class JobsOrchestrator
    {
        [FunctionName("JobsOrchestrator")]
        public static async Task<string> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context, ILogger log)
        {
            string output = "";
            JObject jobProperties = JObject.Parse(context.GetInput<string>());
            int duration = (int) jobProperties["duration"];
            int frequency = (int) jobProperties["frequency"];
            int secondsPerBatch = Convert.ToInt16(Environment.GetEnvironmentVariable("secondsPerBatch"));
            int numOfBatches = duration * 60 / secondsPerBatch;
            int numOfMessages = frequency * 60 * duration;
            var endTime = context.CurrentUtcDateTime.AddMinutes(duration);

            while (context.CurrentUtcDateTime < endTime) {
                var nextBatchTime = context.CurrentUtcDateTime.AddSeconds(secondsPerBatch);
        
                switch ((string) jobProperties["messageMethod"])
                {
                    case "eventhub":
                        await context.CallActivityAsync<string>("Job_SendEventHubMessageBatch", jobProperties.ToObject<EventHubJobProperties>());
                        break;
                    case "storagequeue":
                        output = await context.CallActivityAsync<string>("Job_SendStorageQueueMessageBatch", jobProperties.ToObject<JobProperties>());
                        break;
                    case "servicebus":
                        output = await context.CallActivityAsync<string>("Job_SendServiceBusMessageBatch", jobProperties.ToObject<ServiceBusJobProperties>());
                        break;
                    case "eventgrid":
                        await context.CallActivityAsync<string>("Job_SendEventGridMessageBatch", jobProperties.ToObject<EventGridJobProperties>());
                        break;
                    default:
                        output = "invalid messageMethod";
                        break;
                }

                await context.CreateTimer(nextBatchTime, CancellationToken.None);
            }

            log.LogInformation($"finished sending {numOfMessages} messages");
            return $"finished sending {numOfMessages} messages to your {jobProperties["messageMethod"]}";
        }
    }
}
