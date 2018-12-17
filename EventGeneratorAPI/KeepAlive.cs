
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace EventGeneratorAPI.models
{
    public static class KeepAlive
    {
        [FunctionName("KeepAlive")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"KeepAlive Timer trigger function executed");
        }
    }
}
