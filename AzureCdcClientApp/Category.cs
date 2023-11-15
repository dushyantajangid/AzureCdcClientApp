using AzureCdcClientApp.Interface;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;

namespace AzureCdcClientApp
{
    public class Category
    {
        private readonly ISqlTextQuery _sqlTextQuery;
        private readonly IConfigurationSections _config;
        private readonly ILogger<Category> _logger;
        private readonly TelemetryClient _telemetryClient;
        private readonly string _sqlDbConnectionString;

        public Category(IKeyVaultAccess keyVaultAccess, ISqlTextQuery sqlTextQuery, IConfigurationSections config, TelemetryClient telemetryClient, ILogger<Category> logger)
        {
            _sqlTextQuery = sqlTextQuery;
            _config = config;
            _telemetryClient = telemetryClient;
            _logger = logger;
            _sqlDbConnectionString = keyVaultAccess.GetDestinationDbConnection();
        }

        public bool ProcessCategory(Dictionary<string, string> cdcDetail)
        {
            using (_telemetryClient.StartOperation<RequestTelemetry>("Category_ProcessCategory_operation"))
            {
                _telemetryClient.TrackEvent("Category:ProcessCategory started");

                string categoryId;
                string categoryName;
                string destinationUpsertQuery;
                bool isSuccess = false;
                string operationType;

                cdcDetail.TryGetValue("__$operation", out operationType);
                cdcDetail.TryGetValue("category_id", out categoryId);
                cdcDetail.TryGetValue("category_name", out categoryName);

                switch (operationType)
                {
                    case "1":
                        destinationUpsertQuery = "DELETE FROM [dbo].[Categories] WHERE category_id=" + categoryId;
                        isSuccess = UpsertCategory(destinationUpsertQuery);
                        break;
                    case "2":
                        destinationUpsertQuery = "INSERT INTO [dbo].[Categories] ([category_id],[category_name])  VALUES (" + categoryId + ",'" + categoryName + "')";
                        isSuccess = UpsertCategory(destinationUpsertQuery);
                        break;
                    case "4":
                        destinationUpsertQuery = "UPDATE [dbo].[Categories] SET [category_name]='" + categoryName + "' WHERE [category_id] =" + categoryId;
                        isSuccess = UpsertCategory(destinationUpsertQuery);
                        break;
                }
                _telemetryClient.TrackEvent("Category:ProcessCategory completed");
                return isSuccess;
            }
        }
        public bool UpsertCategory(string destinationUpsertQuery)
        {
            using (_telemetryClient.StartOperation<RequestTelemetry>("Category_UpsertCategory_operation"))
            {
                _telemetryClient.TrackEvent("Category:UpsertCategory started");

                try
                {
                    var queryResult = _sqlTextQuery.PerformQuery(destinationUpsertQuery, _sqlDbConnectionString);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Category:UpsertCategory: Error occured");
                    return false;
                }
                finally
                {
                    _telemetryClient.TrackEvent("Category:UpsertCategory completed");
                }
            }

        }
    }
}