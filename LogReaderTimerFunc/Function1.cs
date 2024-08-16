using System;
using Azure;
using Azure.Identity;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace LogReaderTimerFunc
{
    public class LogReaderTimerFunc
    {
        private readonly ILogger _logger;

        public LogReaderTimerFunc(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<LogReaderTimerFunc>();
        }

        [Function("LogReaderTimerFunc")]
        public void Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer)
        {
            FetchLogs();
        }



    }

}
