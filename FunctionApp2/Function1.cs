using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FunctionApp2
{
    public class Function2
    {
        private readonly ILogger _logger;

        public Function2(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Function2>();
        }

        [Function("Function2")]
        public void Run([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer)
        {
            _logger.LogCritical($"Function2 critical trigger function executed at: {DateTime.Now}");
            
            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }
        }
    }
}
