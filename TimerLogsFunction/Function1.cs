using Azure;
using Azure.Identity;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Monitor;
using Azure.ResourceManager.ApplicationInsights;
using Azure.Core;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Azure.ResourceManager.ApplicationInsights.Mocking;
using Azure.ResourceManager.OperationalInsights;

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
            logqueryAsync();
        }
        void FetchLogs()
        {
            try
            {
                
                ArmClient armclient = new ArmClient(new DefaultAzureCredential());
                
                var subscriptionResources = armclient.GetSubscriptions();
                
                
                foreach (var subscriptionResource in subscriptionResources)
                {
                    foreach (var applicationInsight in subscriptionResource.GetApplicationInsightsComponents().ToList())
                    {
                        Console.WriteLine(applicationInsight.Data.AppId);
                        
                        
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        void logqueryAsync()
        {
            var client = new LogsQueryClient(new DefaultAzureCredential());

            string resourceId = "/subscriptions/d85e5f9e-6cdc-4347-8923-829300a15ec9/resourceGroups/Test2/providers/microsoft.insights/components/LoggerFunc";
            string tableName = "traces";
            Response<LogsQueryResult> results = client.QueryResource(
                new ResourceIdentifier(resourceId),
                $"traces | where SeverityLevel between (1 ..4 )  | top 1 by TimeGenerated",
                            new QueryTimeRange(TimeSpan.FromMinutes(30)));    

            LogsTable resultTable = results.Value.Table;
            foreach (LogsTableRow row in resultTable.Rows)
            {
                Console.WriteLine($"{row["OperationName"]} {row["ResourceGroup"]}");
            }

            foreach (LogsTableColumn columns in resultTable.Columns)
            {
                Console.WriteLine("Name: " + columns.Name + " Type: " + columns.Type);
            }
        }

    //    void QueryWorkspaces(ApplicationInsightsComponentResource applicationInsight)
    //    {
            
    //        var queryClient = new LogsQueryClient(new DefaultAzureCredential());

    //        queryClient.QueryResource(new DefaultAzureCredential());

    //        Response<LogsQueryResult> result = client.QueryWorkspace(
    //            workspaceId,
    //            "AppTraces | where SeverityLevel between (1 ..4 )  | top 1 by TimeGenerated",
    //            new QueryTimeRange(TimeSpan.FromMinutes(30)));            

    //        LogsTable table = result.Value.Table;
    //        foreach (var row in table.Rows)
    //        {
    //            Console.WriteLine(row.ToString());
    //            table.Columns.ToList().ForEach(x =>
    //            {
    //                Console.WriteLine($"{x.Name} - {x.Type} {row[x.Name]}");
    //            });
    //            _logger.LogInformation($"{row["OperationName"]} {row["SeverityLevel"]} {row["_ResourceId"]}");
    //        }
    //    }
    }
}

