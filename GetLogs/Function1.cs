using Azure;
using Azure.Identity;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GetLogs
{
    public class AzureFuncGetLogs
    {
        private readonly ILogger<AzureFuncGetLogs> _logger;

        public AzureFuncGetLogs(ILogger<AzureFuncGetLogs> logger)
        {
            _logger = logger;
        }

        [Function("Function1")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            FetchLogs();
            return new OkObjectResult("Welcome to Azure GetLoggs!");
        }


         
    }
}
