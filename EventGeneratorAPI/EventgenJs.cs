using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Net.Http.Headers;
using System.IO;

namespace EventGeneratorAPI
{
    public static class EventgenJs
    {
        [FunctionName("EventgenJs")]
        public static HttpResponseMessage Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "eventgen.js")]HttpRequestMessage req, TraceWriter log)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new FileStream(@"d:\home\site\wwwroot\www\eventgen.js", FileMode.Open, FileAccess.Read, FileShare.Read);
            //var stream = new FileStream(@"C:\Projects\EventGenerator\EventGeneratorAPI\www\eventgen.js", FileMode.Open);
            response.Content = new StreamContent(stream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/javascript");
            return response;
        }
    }
}
