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
        static IServiceProvider _serviceProvider;
        static ILoggerFactory _loggerFactory;
        static ILogger<Program> _programLogger;
        static IConfigurationSections _config;
        static IKeyVaultAccess _keyVaultAccess;
        static TelemetryClient _telemetryClient;
        static IPrintOutput _printOutput;

        static Category _category;
        static Product _product;
        static CdcConfiguration _cdcConfiguration;
        static CdcData _cdcData;

        static void Main()
        {
            string newOffset;
            bool success;

            StartupInitialization();
            _programLogger.LogInformation("Program:Main Started");

            ServiceInitialization();

            _printOutput?.WelcomeMessage();

            if (_cdcConfiguration is not null)
            {
                IEnumerable<Dictionary<string, string>> cdcConfiguration = _cdcConfiguration.GetCdcConfigurationData();

                if (cdcConfiguration is not null && cdcConfiguration.Any())
                {
                    _printOutput.AllCdcConfiguration(cdcConfiguration);

                    foreach (var cdcTable in cdcConfiguration)
                    {
                        CdcConfigurationModel cdcObj = cdcTable.ToObject<CdcConfigurationModel>();

                        _programLogger.LogDebug("Program: Main Processing table: " + cdcObj.table_name);

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

                                _cdcConfiguration.UpdateCdcOffsetData(cdcObj, cdcDetail.GetValue("start_lsn_string"));
                            }
                        }
                        _cdcConfiguration.UpdateCdcLastDateCheckData(cdcObj);
                        _programLogger.LogDebug("Program: Main Processed table: " + cdcObj.table_name);
                        _printOutput.TableProcessedMessage(cdcObj.table_name);

                    }
                }
                else
                    _printOutput.CdcConfigurationMissing();
            }

            _printOutput?.EndMessage();
            _telemetryClient?.Flush();
            _programLogger.LogInformation("Program:Main Completed");

        }

        private static void ServiceInitialization()
        {
            ILogger<Product> _productLogger;
            ILogger<Category> _categoryLogger;
            ILogger<CdcConfiguration> _cdcConfigurationLogger;
            ILogger<CdcData> _cdcDataLogger;
            ILogger<KeyVaultAccess> _keyVaultLogger;
            ILogger<ServiceBuilder> _serviceBuilderLogger;

            SqlTextQuery _sqlTextQuery;

            try
            {
                _programLogger.LogInformation("Program:ServiceInitialization Started");

                _keyVaultLogger = _loggerFactory.CreateLogger<KeyVaultAccess>();
                _serviceBuilderLogger = _loggerFactory.CreateLogger<ServiceBuilder>();
                _cdcConfigurationLogger = _loggerFactory.CreateLogger<CdcConfiguration>();
                _cdcDataLogger = _loggerFactory.CreateLogger<CdcData>();
                _productLogger = _loggerFactory.CreateLogger<Product>();
                _categoryLogger = _loggerFactory.CreateLogger<Category>();

                _keyVaultAccess = new KeyVaultAccess(_config, _keyVaultLogger);
                _serviceProvider = new ServiceBuilder(_config, _keyVaultAccess, _serviceBuilderLogger).ServiceProvider();
                _printOutput = _serviceProvider.GetRequiredService<PrintOutput>();

                _telemetryClient = _serviceProvider.GetRequiredService<TelemetryClient>();
                _sqlTextQuery = _serviceProvider.GetRequiredService<SqlTextQuery>();

                _cdcConfiguration = new CdcConfiguration(_keyVaultAccess, _sqlTextQuery, _config, _telemetryClient, _cdcConfigurationLogger);
                _cdcData = new CdcData(_keyVaultAccess, _sqlTextQuery, _config, _telemetryClient, _cdcDataLogger);

                _category = new Category(_keyVaultAccess, _sqlTextQuery, _config, _telemetryClient, _categoryLogger);
                _product = new Product(_keyVaultAccess, _sqlTextQuery, _config, _telemetryClient, _productLogger);
            }
            catch (Exception ex)
            {
                _programLogger.LogDebug(ex, "Program:ServiceInitialization Couldn't initiate services");
                _programLogger.LogCritical("Program:ServiceInitialization Couldn't initiate services");
            }
            _programLogger.LogInformation("Program:ServiceInitialization Completed");
        }

        private static void StartupInitialization()
        {
            _config = new Configuration();
            _loggerFactory = new LoggingBuilder(_config).BuildLogger();

            _programLogger = _loggerFactory.CreateLogger<Program>();
        }
    }

}