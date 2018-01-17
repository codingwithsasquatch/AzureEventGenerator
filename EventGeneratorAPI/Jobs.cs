//#r "Newtonsoft.Json"
//#r "Microsoft.WindowsAzure.Storage"
//#r "Microsoft.ServiceBus"

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.EventHubs;
using System.Text;
using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.Azure.ServiceBus;
using System.Net.Http.Headers;
using System.IO;

namespace EventGeneratorAPI
{
    public class JobProperties
    {
        public int frequency { get; set; }
        public int duration { get; set; }
        public string messageScheme { get; set; }
    }

    public class EventHubJobProperties : JobProperties
    {
        public string connectionString { get; set; }
        public string eventhub { get; set; }
    }

    public class StorageQueueJobProperties : JobProperties
    {
        public string connectionString { get; set; }
        public string queue { get; set; }
    }

    public class ServiceBusJobProperties : JobProperties
    {
        public string connectionString { get; set; }
        public string queue { get; set; }
    }
    public class EventGridJobProperties : JobProperties
    {
        public string endpoint { get; set; }
        public string key { get; set; }
    }

    public static class Jobs
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


        [FunctionName("Job_StorageQueueMessageGenerator")]
        public static async Task<string> StorageQueueMessageGenerator([ActivityTrigger] StorageQueueJobProperties sqJobProperties, TraceWriter log)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(sqJobProperties.connectionString);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference(sqJobProperties.queue);
            await queue.CreateIfNotExistsAsync();

            int numOfMessages = sqJobProperties.frequency * 60 * sqJobProperties.duration;
            string[] messages = CreateMessages(numOfMessages, sqJobProperties.messageScheme);

            for (var i = 0; i < numOfMessages; i++)
            {
                try
                {
                    var message = messages[i]; //CreateMessage(numOfMessages, sqJobProperties.messageScheme);
                    log.Info($"Sending message: {message}");
                    CloudQueueMessage cloudQueueMessage = new CloudQueueMessage(message);
                    await queue.AddMessageAsync(cloudQueueMessage);
                }
                catch (Exception exception)
                {
                    log.Info($"Exception: {exception.Message}");
                }
                await Task.Delay(1000 / sqJobProperties.frequency);
            }

            log.Info($"finished.");
            return $"finished sending {numOfMessages} to {sqJobProperties.queue}!";
        }

        [FunctionName("Job_EventHubMessageGenerator")]
        public static async Task<string> EventHubMessageGenerator([ActivityTrigger] EventHubJobProperties ehJobProperties, TraceWriter log)
        {
            EventHubsConnectionStringBuilder connectionStringBuilder = new EventHubsConnectionStringBuilder(ehJobProperties.connectionString)
            {
                EntityPath = ehJobProperties.eventhub
            };
            EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
            int numOfMessages = ehJobProperties.frequency * 60 * ehJobProperties.duration;
            string[] messages = CreateMessages(numOfMessages, ehJobProperties.messageScheme);
            for (var i = 0; i < numOfMessages; i++)
            {
                try
                {
                    var message = messages[i]; //CreateMessage(i, ehJobProperties.messageScheme);
                    log.Info($"Sending message: {message}");
                    await eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(message)));
                }
                catch (Exception exception)
                {
                    log.Info($"Exception: {exception.Message}");
                }
                await Task.Delay(1000/ehJobProperties.frequency);
            }

            await eventHubClient.CloseAsync();

            log.Info($"finished.");
            return $"finished sending {numOfMessages} to {ehJobProperties.eventhub}!";
        }

        [FunctionName("Job_ServiceBusMessageGenerator")]
        public static async Task<string> ServiceBusMessageGenerator([ActivityTrigger] ServiceBusJobProperties sbJobProperties, TraceWriter log)
        {
            QueueClient queueClient = new QueueClient(sbJobProperties.connectionString, sbJobProperties.queue);

            int numOfMessages = sbJobProperties.frequency * 60 * sbJobProperties.duration;
            string[] messages = CreateMessages(numOfMessages, sbJobProperties.messageScheme);
            for (var i = 0; i < numOfMessages; i++)
            {
                try
                {
                    var message = messages[i]; //CreateMessage(i, sbJobProperties.messageScheme);
                    log.Info($"Sending message: {message}");
                    var body = Encoding.UTF8.GetBytes(message);
                    await queueClient.SendAsync(new Message { Body = body, ContentType = "text/plain"});
                }
                catch (Exception exception)
                {
                    log.Info($"Exception: {exception.Message}");
                }
                await Task.Delay(1000 / sbJobProperties.frequency);
            }
            await queueClient.CloseAsync();

            log.Info($"finished.");
            return $"finished sending {numOfMessages} to {sbJobProperties.queue}!";
        }

        [FunctionName("Job_EventGridMessageGenerator")]
        public static async Task<string> EventGridMessageGenerator([ActivityTrigger] EventGridJobProperties egJobProperties, TraceWriter log)
        {
            var client = new HttpClient { BaseAddress = new Uri(egJobProperties.endpoint) };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("aeg-sas-key", egJobProperties.key);

            int numOfMessages = egJobProperties.frequency * 60 * egJobProperties.duration;
            string[] messages = CreateMessages(numOfMessages, egJobProperties.messageScheme);
            for (var i = 0; i < numOfMessages; i++)
            {
                try
                {
                    var message = messages[i]; //CreateMessage(i, egJobProperties.messageScheme);
                    log.Info($"Sending message {i}: {message}");
                    var data = new[]
                    {
                        new
                        {
                            Subject = egJobProperties.messageScheme.ToLower(),
                            EventType = egJobProperties.messageScheme.ToLower(),
                            EventTime = DateTime.UtcNow,
                            Id = Guid.NewGuid().ToString(),
                            Data = new
                            {
                                message
                            }
                        }
                    };
                    var response = await client.PostAsJsonAsync(string.Empty, data);
                }
                catch (Exception exception)
                {
                    log.Info($"Exception: {exception.Message}");
                }
                await Task.Delay(1000 / egJobProperties.frequency);
            }

            log.Info($"finished.");
            return $"finished sending {numOfMessages} to {egJobProperties.endpoint}!";
        }

        private static string[] CreateMessages(int numOfMessages, string scheme)
        {
            string[] messages = new string[numOfMessages];

            switch (scheme.ToLower())
            {
                case "ninjaattack":
                    dynamic ninjaAttack = JObject.Parse(File.ReadAllText(@"..\..\..\NinjaAttack.json"));
                    Random random = new Random();

                    for (int i=0; i<numOfMessages; i++)
                    {
                        //decide if the good or bad ninja will be the actor and pick which good and bad ninjas will be involved
                        bool goodNinjaActor = random.Next(1) == 1 ? true : false;
                        int goodNinjaIndex = random.Next(ninjaAttack.goodNinjas.Count);
                        int badNinjaIndex = random.Next(ninjaAttack.badNinjas.Count);
                        string actor = goodNinjaActor ? (string)ninjaAttack.goodNinjas[goodNinjaIndex] : (string)ninjaAttack.badNinjas[badNinjaIndex];
                        //whoever isn't the actor is the actee
                        string target = goodNinjaActor ? (string)ninjaAttack.badNinjas[badNinjaIndex] : (string)ninjaAttack.goodNinjas[badNinjaIndex];

                        //get the index of the weapon to be used so we can grab the properties
                        int weaponIndex = random.Next(ninjaAttack.weapons.Count);
                        string weapon = ninjaAttack.weapons[weaponIndex].name;

                        //get the index of the action for the weapon to be used so we can grab the properties
                        int actionIndex = random.Next(ninjaAttack.weapons[weaponIndex].actions.Count);
                        string action = ninjaAttack.weapons[weaponIndex].actions[actionIndex].name;
                        int points = ninjaAttack.weapons[weaponIndex].actions[actionIndex].points;

                        var data = new
                        {
                            actor,
                            side = goodNinjaActor ? "good" : "bad",
                            weapon,
                            action,
                            target,
                            points,
                            description = $"Message {i}: {actor} {action} {target} with {weapon} for {points} points"
                        };

                        messages[i] = JsonConvert.SerializeObject(data);
                    }
                    break;
                default:
                    for (int i=0; i<numOfMessages; i++)
                    {
                        var data = new { message = $"Message {i}" };
                        messages[i] = JsonConvert.SerializeObject(data);
                    }
                    break;
            }
            return messages;
        }

        [FunctionName("JobStart")]
        public static async Task<HttpResponseMessage> HttpJobs(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "job")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient JobsOrchestrator,
            TraceWriter log)
        {
            string jobProperties = await req.Content.ReadAsStringAsync();
            string jobId = await JobsOrchestrator.StartNewAsync("JobsOrchestrator", jobProperties);
            log.Info($"Started orchestration with ID = '{jobId}' and the following properties: {jobProperties}");
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(jobId) };
        }

        [FunctionName("Job")]
        public static async Task<HttpResponseMessage> HttpJob(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get","delete", Route = "job/{jobId}")]HttpRequestMessage req,
            string jobId,
            [OrchestrationClient]DurableOrchestrationClient JobsOrchestrator,
            TraceWriter log)
        {
            switch (req.Method.ToString())
            {
                //get status of job
                case "GET":
                    var status = await JobsOrchestrator.GetStatusAsync(jobId);
                    if (status != null)
                    {
                        var tmp = new { id = status.InstanceId, status = status.RuntimeStatus, output = status.Output != null ? status.Output.ToString() : "" };
                        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonConvert.SerializeObject(tmp)) };
                    } else
                    {
                        return new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent(jobId) };
                    }
                //delete job
                case "DELETE":
                    await JobsOrchestrator.TerminateAsync(jobId, "user requested termination");
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(jobId) };
                default:
                    return new HttpResponseMessage(HttpStatusCode.NotImplemented) { Content = new StringContent(jobId) };
            }
        }

        [FunctionName("Gui")]
        public static HttpResponseMessage IndexHtml(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "gui")]HttpRequestMessage req, TraceWriter log)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new FileStream(@"d:\home\site\wwwroot\www\index.html", FileMode.Open, FileAccess.Read, FileShare.Read);
            //var stream = new FileStream(@"C:\Projects\EventGenerator\EventGeneratorAPI\www\index.html", FileMode.Open);
            response.Content = new StreamContent(stream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }

        [FunctionName("EventgenJS")]
        public static HttpResponseMessage EventgenJS(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "eventgen.js")]HttpRequestMessage req, TraceWriter log)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new FileStream(@"d:\home\site\wwwroot\www\eventgen.js", FileMode.Open, FileAccess.Read, FileShare.Read);
            //var stream = new FileStream(@"C:\Projects\EventGenerator\EventGeneratorAPI\www\eventgen.js", FileMode.Open);
            response.Content = new StreamContent(stream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/javascript");
            return response;
        }

        [FunctionName("KeepAlive")]
        public static void KeepAlive([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"KeepAlive Timer trigger function executed");
        }
    }
}