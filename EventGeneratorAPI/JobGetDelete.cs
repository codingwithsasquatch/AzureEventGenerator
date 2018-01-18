﻿using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace EventGeneratorAPI
{
    public static class JobGetDelete
    {
        [FunctionName("JobGetDelete")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "delete", Route = "job/{jobId}")]HttpRequestMessage req,
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
                    }
                    else
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
    }
}