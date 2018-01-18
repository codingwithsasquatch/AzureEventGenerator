using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace EventGeneratorAPI
{
    public static class JobCreate
    {
        [FunctionName("JobCreate")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "job")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient JobsOrchestrator,
            TraceWriter log)
        {
            string jobProperties = await req.Content.ReadAsStringAsync();
            string jobId = await JobsOrchestrator.StartNewAsync("JobsOrchestrator", jobProperties);
            log.Info($"Started orchestration with ID = '{jobId}' and the following properties: {jobProperties}");
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(jobId) };
        }
    }
}
