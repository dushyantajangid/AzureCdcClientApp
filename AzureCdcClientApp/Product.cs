﻿using AzureCdcClientApp.Extensions;
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

                string operationType = cdcDetail.GetValue("__$operation");
                string productId = cdcDetail.GetValue("product_id");
                string productName = cdcDetail.GetValue("product_name");
                string categoryId = cdcDetail.GetValue("category_id");
                string price = cdcDetail.GetValue("price");
                string description = cdcDetail.GetValue("description");
                string imageUrl = cdcDetail.GetValue("image_url");
                string dateAdded = cdcDetail.GetValue("date_added");
                bool isSuccess = false;
                string destinationUpsertQuery;

                _logger.LogDebug("Product: ProcessProduct Processing operationType: " + operationType + " productId: " + productId + " productName: " + productName +
                 " categoryId: " + categoryId + " price: " + price + " description: " + description + " imageUrl: " + imageUrl + " dateAdded: " + dateAdded);

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
                    _logger.LogDebug(ex, "Product:UpsertProduct");
                    _logger.LogError("Product:UpsertProduct: Error occured");
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