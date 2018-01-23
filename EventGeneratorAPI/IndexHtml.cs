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
    public static class IndexHtml
    {
        [FunctionName("IndexHtml")]
        public static HttpResponseMessage Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "gui")]HttpRequestMessage req, TraceWriter log)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            
            var stream = new FileStream(Path.Combine(System.Environment.GetEnvironmentVariable("HOME", EnvironmentVariableTarget.Process), @"site\wwwroot\www\index.html"), FileMode.Open, FileAccess.Read, FileShare.Read);
            //var stream = new FileStream(System.IO.Path.GetFullPath(@"www\index.html"), FileMode.Open, FileAccess.Read, FileShare.Read);
            response.Content = new StreamContent(stream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}
