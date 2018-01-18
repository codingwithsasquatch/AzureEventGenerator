using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Net.Http.Headers;
using System.IO;

namespace EventGeneratorAPI
{
    public static class IndexHtml
    {
        [FunctionName("IndexHtml")]
        public static HttpResponseMessage Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "gui")]HttpRequestMessage req, TraceWriter log)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new FileStream(@"d:\home\site\wwwroot\www\index.html", FileMode.Open, FileAccess.Read, FileShare.Read);
            //var stream = new FileStream(@"C:\Projects\EventGenerator\EventGeneratorAPI\www\index.html", FileMode.Open);
            response.Content = new StreamContent(stream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}
