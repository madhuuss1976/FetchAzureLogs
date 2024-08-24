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
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.ConnectedVMwarevSphere;
using Azure.ResourceManager.Compute;
using Microsoft.Graph;


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

        }

        void GetGraphClient(string appid)
        {
            var scopes = new[] { "User.Read" };

            // Multi-tenant apps can use "common",
            // single-tenant apps must use the tenant ID from the Azure portal
            var tenantId = "common";

            // Value from app registration
            var clientId = appid;

            // using Azure.Identity;
            var options = new DeviceCodeCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
                ClientId = clientId,
                TenantId = tenantId,
                // Callback function that receives the user prompt
                // Prompt contains the generated device code that user must
                // enter during the auth process in the browser
                DeviceCodeCallback = (code, cancellation) =>
                {
                    Console.WriteLine(code.Message);
                    return Task.FromResult(0);
                },
            };

            // https://learn.microsoft.com/dotnet/api/azure.identity.devicecodecredential
            var deviceCodeCredential = new DeviceCodeCredential(options);

            var graphClient = new GraphServiceClient(deviceCodeCredential, scopes);

            // Code snippets are only available for the latest version. Current version is 5.x

            // To initialize your graphClient, see https://learn.microsoft.com/en-us/graph/sdks/create-client?from=snippets&tabs=csharp
            var result = graphClient.Applications.GetAsync();
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
                        GetGraphClient(applicationInsight.Data.ApplicationId);
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
