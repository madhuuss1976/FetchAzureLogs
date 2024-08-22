using Azure;
using Azure.Communication.Email;
using Azure.Identity;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.ResourceManager;
using Azure.ResourceManager.ApplicationInsights;
using Azure.Core;

namespace TimerLogsFunction
{
    public class TimerLogsFunction
    {
        private readonly ILogger _logger;
        DefaultAzureCredential _credential;

        public TimerLogsFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TimerLogsFunction>();
        }

        [Function("TimerLogsFunction")]
        public void Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer)
        {
            _credential = new DefaultAzureCredential();
            FetchLogs();
        }
        void FetchLogs()
        {
            try
            {
                string tableName = "traces";
                string query = "where severityLevel between (1 ..4 )";

                ArmClient armclient = new ArmClient(_credential);
                var subscriptionResources = armclient.GetSubscriptions();                
                foreach (var subscriptionResource in subscriptionResources)
                {
                    foreach (var applicationInsight in subscriptionResource.GetApplicationInsightsComponents().ToList())
                    {
                        logqueryAsync(applicationInsight.Data.Id, tableName, query);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }


        void logqueryAsync(string resourceId, string tableName, string query)
        {
            var client = new LogsQueryClient(_credential);
            Response<LogsQueryResult> results = client.QueryResource(new ResourceIdentifier(resourceId), $"{tableName} | {query}",
                        new QueryTimeRange(TimeSpan.FromMinutes(30)));            
            var resultTable = results.Value.Table;
            foreach (LogsTableRow row in resultTable.Rows.Take(10))
            {
                foreach (LogsTableColumn columns in resultTable.Columns)
                {
                    Console.WriteLine($"{columns.Name} -  {row[columns.Name]}");
                    //SendEmail();
                }
            }
        }
        void SendEmail()
        {
            // This code retrieves your connection string from an environment variable.
            string connectionString = Environment.GetEnvironmentVariable("COMMUNICATION_SERVICES_CONNECTION_STRING");
            var emailClient = new EmailClient(connectionString);

            EmailSendOperation emailSendOperation = emailClient.Send(
                WaitUntil.Completed,
                senderAddress: "DoNotReply@<from_domain>",
                recipientAddress: "<to_email>",
                subject: "Test Email",
                htmlContent: "<html><h1>Hello world via email.</h1l></html>",
                plainTextContent: "Hello world via email.");

        }
    }
}
