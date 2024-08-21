using Azure;
using Azure.Identity;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Monitor;
using Azure.ResourceManager.OperationalInsights;
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
            FetchLogs();
        }
        void FetchLogs()
        {
            try
            {
                ArmClient armclient = new ArmClient(new DefaultAzureCredential());
                var subscriptionResources = armclient.GetSubscriptions();
                foreach (var subscriptionResource in subscriptionResources)
                {
                    foreach (var workspace in subscriptionResource.GetOperationalInsightsWorkspaces().ToList())
                    {                        
                        QueryWorkspaces(workspace.Data.CustomerId.ToString());

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }


        void QueryWorkspaces(string workspaceId)
        {
            var client = new LogsQueryClient(new DefaultAzureCredential());
            Response<LogsQueryResult> result = client.QueryWorkspace(
                workspaceId,
                "AppTraces | where SeverityLevel between (1 ..4 )  | top 1 by TimeGenerated",
                new QueryTimeRange(TimeSpan.FromMinutes(30)));

            LogsTable table = result.Value.Table;
            foreach (var row in table.Rows)
            {
                Console.WriteLine(row.ToString());
                table.Columns.ToList().ForEach(x =>
                {
                    Console.WriteLine($"{x.Name} - {x.Type} {row[x.Name]}");
                });
                _logger.LogInformation($"{row["OperationName"]} {row["SeverityLevel"]} {row["_ResourceId"]}");
            }
        }
    }
}

