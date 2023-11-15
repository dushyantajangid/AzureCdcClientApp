using AzureCdcClientApp.Interface;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;

namespace AzureCdcClientApp
{
    public class Product
    {
        private readonly ISqlTextQuery _sqlTextQuery;
        private readonly IConfigurationSections _config;
        private readonly ILogger<Product> _logger;
        private readonly TelemetryClient _telemetryClient;
        private readonly string _sqlDbConnectionString;

        public Product(IKeyVaultAccess keyVaultAccess, ISqlTextQuery sqlTextQuery, IConfigurationSections config, TelemetryClient telemetryClient, ILogger<Product> logger)
        {
            _sqlTextQuery = sqlTextQuery;
            _config = config;
            _telemetryClient = telemetryClient;
            _logger = logger;
            _sqlDbConnectionString = keyVaultAccess.GetDestinationDbConnection();
        }

        public bool ProcessProduct(Dictionary<string, string> cdcDetail)
        {
            using (_telemetryClient.StartOperation<RequestTelemetry>("Product_ProcessProduct_operation"))
            {
                _telemetryClient.TrackEvent("Product:ProcessProduct started");
                string categoryId;
                string dateAdded;
                string description;
                string destinationUpsertQuery;
                string imageUrl;
                bool isSuccess = false;

                string operationType;
                string price;
                string productId;
                string productName;

                cdcDetail.TryGetValue("__$operation", out operationType);
                cdcDetail.TryGetValue("product_id", out productId);
                cdcDetail.TryGetValue("product_name", out productName);
                cdcDetail.TryGetValue("category_id", out categoryId);
                cdcDetail.TryGetValue("price", out price);
                cdcDetail.TryGetValue("description", out description);
                cdcDetail.TryGetValue("image_url", out imageUrl);
                cdcDetail.TryGetValue("date_added", out dateAdded);

                switch (operationType)
                {
                    case "1":
                        destinationUpsertQuery = "DELETE FROM [dbo].[Products] WHERE product_id=" + productId;
                        isSuccess = UpsertProduct(destinationUpsertQuery);
                        break;
                    case "2":
                        destinationUpsertQuery = "INSERT INTO [dbo].[Products] ([product_id],[product_name],[category_id],[price],[description],[image_url])  " +
                            "VALUES (" + productId + ",'" + productName + "'," + categoryId + "," + price + ",'" + description + "','" + imageUrl + "')";
                        isSuccess = UpsertProduct(destinationUpsertQuery);
                        break;
                    case "4":
                        destinationUpsertQuery = "UPDATE [dbo].[Products] SET [product_name]='" + productName + "', [category_id]= " + categoryId + ",[price]= " + price +
                            ",[description]= '" + description + "',[image_url]= '" + imageUrl + "' WHERE [product_id] =" + productId;
                        isSuccess = UpsertProduct(destinationUpsertQuery);
                        break;
                }

                return isSuccess;
            }
        }
        public bool UpsertProduct(string destinationUpsertQuery)
        {
            using (_telemetryClient.StartOperation<RequestTelemetry>("Product_UpsertProduct_operation"))
            {
                _telemetryClient.TrackEvent("Product:UpsertProduct started");

                try
                {
                    var queryResult = _sqlTextQuery.PerformQuery(destinationUpsertQuery, _sqlDbConnectionString);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Product:UpsertProduct: Error occured");
                    return false;
                }
                finally
                {
                    _telemetryClient.TrackEvent("Product:UpsertProduct completed");
                }
            }

        }
    }
}