
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json.Linq;
using EventGeneratorAPI.Models;

namespace EventGeneratorAPI
{
    public static class JobsOrchestrator
    {
        [FunctionName("JobsOrchestrator")]
        public static async Task<string> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            string output;
            JObject jobProperties = JObject.Parse(context.GetInput<string>());

            switch ((string)jobProperties["messageMethod"])
            {
                case "eventhub":
                    output = await context.CallActivityAsync<string>("Job_EventHubMessageGenerator", jobProperties.ToObject<EventHubJobProperties>());
                    break;
                case "storagequeue":
                    output = await context.CallActivityAsync<string>("Job_StorageQueueMessageGenerator", jobProperties.ToObject<EventHubJobProperties>());
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
            return output;
        }
    }
}
