using Azure;
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

        public TimerLogsFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TimerLogsFunction>();
        }

        [Function("TimerLogsFunction")]
        public void Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer)
        {
            try
            {
                FetchLogs();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

        }
        void FetchLogs()
        {
            var defaultAzureCredentials = new DefaultAzureCredential();
            ArmClient armclient = new ArmClient(defaultAzureCredentials);
            var subscriptionResources = armclient.GetSubscriptions();
            foreach (var subscriptionResource in subscriptionResources)
            {
                foreach (var applicationInsight in subscriptionResource.GetApplicationInsightsComponents().ToList())
                {
                    QueryTable(defaultAzureCredentials, applicationInsight.Data.AppId, "traces");
                }
            }
        }
        //string resourceId = "/subscriptions/d85e5f9e-6cdc-4347-8923-829300a15ec9/resourceGroups/LoggerFunc1_group/providers/microsoft.insights/components/LoggerFunc1";
        //string tableName = "traces";
        void QueryTable(DefaultAzureCredential defaultAzureCredentials, string resourceId, string tableName)
        {   
            var resourceid = new ResourceIdentifier(resourceId);
            Response<LogsQueryResult> results = client.QueryResource(resourceid,
                $"traces", new QueryTimeRange(TimeSpan.FromMinutes(30)));
            LogsTable resultTable = results.Value.Table;
            foreach (LogsTableRow row in resultTable.Rows)
            {
                Console.WriteLine($"{row["Operation_Name"]} {row["_ResourceId"]}");
            }
            foreach (LogsTableColumn columns in resultTable.Columns)
            {
                Console.WriteLine("Name: " + columns.Name + " Type: " + columns.Type);
            }
        }
    }
}

