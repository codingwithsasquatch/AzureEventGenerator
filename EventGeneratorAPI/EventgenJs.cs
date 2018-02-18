using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Net.Http.Headers;
using System.IO;
using System;

namespace EventGeneratorAPI
{
    public static class EventgenJs
    {
        [FunctionName("EventgenJs")]
        public static HttpResponseMessage Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "eventgen.js")]HttpRequestMessage req, TraceWriter log)
        {
            FileStream stream;

            if (Environment.GetEnvironmentVariable("isLocal") == "1")
            {
                stream = new FileStream(System.IO.Path.GetFullPath(@"www/eventgen.js"), FileMode.Open, FileAccess.Read, FileShare.Read);
            } else
            {
                stream = new FileStream(Path.Combine(System.Environment.GetEnvironmentVariable("HOME", EnvironmentVariableTarget.Process), @"site\wwwroot\www\eventgen.js"), FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(stream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/javascript");
            return response;
        }
    }
}
