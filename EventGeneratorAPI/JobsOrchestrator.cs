
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
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
            [OrchestrationTrigger] DurableOrchestrationContext context, TraceWriter log)
        {
            const int secondsPerBatch = 5;

            string output = "";
            JObject rawProperties = JObject.Parse(context.GetInput<string>());
            dynamic jobProperties;

            switch ((string)rawProperties["messageMethod"])
            {
                case "eventhub":
                    jobProperties = rawProperties.ToObject<EventHubJobProperties>();
                    break;
                case "storagequeue":
                    jobProperties = rawProperties.ToObject<StorageQueueJobProperties>();
                    break;
                case "servicebus":
                    jobProperties = rawProperties.ToObject<ServiceBusJobProperties>();
                    break;
                case "eventgrid":
                    jobProperties = rawProperties.ToObject<EventGridJobProperties>();
                    break;
                default:
                    return "invalid messageMethod";
            }

            int numOfMessages = jobProperties.Frequency * 60 * jobProperties.Duration;
            IEnumerable<string> messages = Messages.CreateMessages(numOfMessages, jobProperties.MessageScheme);
            int messagesPerBatch = secondsPerBatch * jobProperties.Frequency;
            var batchProperties = jobProperties;
            var nextBatchTime = context.CurrentUtcDateTime.AddSeconds(secondsPerBatch);

            while (messages.Count() > 0)
            {
                batchProperties.Messages = messages.Take(messagesPerBatch);

                switch (jobProperties.MessageMethod)
                {
                    case "eventhub":
                        await context.CallActivityAsync<string>("Job_SendEventHubMessageBatch", (EventHubJobProperties)batchProperties);
                        break;
                    case "storagequeue":
                        output = await context.CallActivityAsync<string>("Job_StorageQueueMessageGenerator", jobProperties.ToObject<JobProperties>());
                        break;
                    case "servicebus":
                        output = await context.CallActivityAsync<string>("Job_ServiceBusMessageGenerator", jobProperties.ToObject<ServiceBusJobProperties>());
                        break;
                    case "eventgrid":
                        output = await context.CallActivityAsync<string>("Job_EventGridMessageGenerator", jobProperties.ToObject<EventGridJobProperties>());
                        break;
                    default:
                        output = "invalid messageMethod";
                        break;
                }

                messages = messages.Skip(messagesPerBatch);
                await context.CreateTimer(nextBatchTime, CancellationToken.None);
            }

            log.Info($"finished sending {numOfMessages} messages");
            return output;
        }
    }
}
