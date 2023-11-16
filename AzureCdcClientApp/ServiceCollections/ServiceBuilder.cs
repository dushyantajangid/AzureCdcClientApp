using AzureCdcClientApp.Interface;
using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AzureCdcClientApp.ServiceCollections
{
    public class ServiceBuilder
    {
        private readonly IConfigurationSections _config;
        private readonly IServiceCollection _services;
        private readonly IKeyVaultAccess _keyVaultAccess;
        private readonly ILogger<ServiceBuilder> _logger;
        public ServiceBuilder(IConfigurationSections config, IKeyVaultAccess keyVaultAccess, ILogger<ServiceBuilder> logger)
        {
            _config = config;
            _keyVaultAccess = keyVaultAccess;
            _logger = logger;
            _services = new ServiceCollection();
        }
        public IServiceProvider ServiceProvider()
        {
            CreateApplicationInsights();
            CreateLoggingService();
            CreateSqlService();
            CreatePrintOutputService();
            return _services.BuildServiceProvider();
        }

        private void CreateApplicationInsights()
        {
            try
            {
                string applicationInsights = _keyVaultAccess.GetApplicationInsightsConnectionString();
                ApplicationInsightsServiceOptions applicationInsightsServiceOptions = new ApplicationInsightsServiceOptions() { ConnectionString = applicationInsights };

                _services.AddApplicationInsightsTelemetryWorkerService(applicationInsightsServiceOptions);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "ServiceBuilder:CreateApplicationInsights");
                _logger.LogError("ServiceBuilder:CreateApplicationInsights: Error occured, Couldn't get keyvault data");
            }
        }

        private void CreateLoggingService()
        {
            try
            {
                _services.AddLogging(loggingBuilder => loggingBuilder.AddFilter<Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider>("CdcClientApp", LogLevel.Information));
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "ServiceBuilder:CreateLoggingService");
                _logger.LogError("ServiceBuilder:CreateLoggingService: Error occured");
            }
        }

        private void CreateSqlService()
        {
            _services.AddSingleton(new SqlTextQuery());
        }

        private void CreatePrintOutputService()
        {
            _services.AddSingleton(new PrintOutput());
        }
    }
}