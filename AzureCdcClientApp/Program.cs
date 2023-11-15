using AzureCdcClientApp.Configurations;
using AzureCdcClientApp.Extensions;
using AzureCdcClientApp.Interface;
using AzureCdcClientApp.Model;
using AzureCdcClientApp.ServiceCollections;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AzureCdcClientApp
{
    internal class Program
    {
        static SqlTextQuery _sqlTextQuery;
        static Category _category;
        static Product _product;
        static CdcConfiguration _cdcConfiguration;
        static CdcData _cdcData;
        static IServiceProvider _serviceProvider;
        static TelemetryClient _telemetryClient;
        static ILogger<Product> _productLogger;
        static ILogger<Category> _categoryLogger;
        static ILogger<CdcConfiguration> _cdcConfigurationLogger;
        static ILogger<CdcData> _cdcLogger;
        static ILogger<KeyVaultAccess> _keyVaultLogger;
        static IConfigurationSections _config;
        static IPrintOutput _printOutput;
        static IKeyVaultAccess _keyVaultAccess;

        static async Task Main()
        {
            string newOffset;
            bool success;

            PrepareService();

            _printOutput.WelcomeMessage();

            IEnumerable<Dictionary<string, string>> cdcConfiguration = _cdcConfiguration.GetCdcConfigurationData();

            _printOutput.AllCdcConfiguration(cdcConfiguration);

            foreach (var cdcTable in cdcConfiguration)
            {
                CdcConfigurationModel cdcObj = cdcTable.ToObject<CdcConfigurationModel>();

                IEnumerable<Dictionary<string, string>> cdcDataCollection = _cdcData.GetCdcData(cdcObj);

                if (cdcDataCollection.Any())
                {
                    foreach (var cdcDetail in cdcDataCollection)
                    {
                        switch (cdcObj.table_name.ToLower())
                        {
                            case "dbo.categories":
                                success = _category.ProcessCategory(cdcDetail);
                                break;

                            case "dbo.products":
                                success = _product.ProcessProduct(cdcDetail);
                                break;
                        }

                        cdcDetail.TryGetValue("start_lsn_string", out newOffset);
                        Console.Write("Processing " + newOffset);
                        _cdcConfiguration.UpdateCdcOffsetData(cdcObj, newOffset);
                    }
                }
                _cdcConfiguration.UpdateCdcLastDateCheckData(cdcObj);
            }

            _telemetryClient.Flush();
            _printOutput.EndMessage();
        }

        private static void PrepareService()
        {
            _config = new Configuration();
            _keyVaultAccess = new KeyVaultAccess(_config);
            _serviceProvider = new ServiceBuilder(_config, _keyVaultAccess).ServiceProvider();

            _productLogger = _serviceProvider.GetRequiredService<ILogger<Product>>();
            _categoryLogger = _serviceProvider.GetRequiredService<ILogger<Category>>();
            _keyVaultLogger = _serviceProvider.GetRequiredService<ILogger<KeyVaultAccess>>();
            _telemetryClient = _serviceProvider.GetRequiredService<TelemetryClient>();
            _sqlTextQuery = _serviceProvider.GetRequiredService<SqlTextQuery>();
            _printOutput = _serviceProvider.GetRequiredService<PrintOutput>();

            _category = new Category(_keyVaultAccess, _sqlTextQuery, _config, _telemetryClient, _categoryLogger);
            _product = new Product(_keyVaultAccess, _sqlTextQuery, _config, _telemetryClient, _productLogger);
            _cdcConfiguration = new CdcConfiguration(_keyVaultAccess, _sqlTextQuery, _config, _telemetryClient, _cdcConfigurationLogger);
            _cdcData = new CdcData(_keyVaultAccess, _sqlTextQuery, _config, _telemetryClient, _cdcLogger);
        }
    }

}