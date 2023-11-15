using AzureCdcClientApp.Interface;
using AzureCdcClientApp.Model;
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
        public ServiceBuilder(IConfigurationSections config, IKeyVaultAccess keyVaultAccess)
        {
            _config = config;
            _services = new ServiceCollection();
            _keyVaultAccess = keyVaultAccess;
        }
        public IServiceProvider ServiceProvider()
        {
            CreateLoggingService();
            CreateApplicationInsights();
            CreateSqlService();
            CreatePrintOutputService();
            return _services.BuildServiceProvider();
        }

        private void CreateApplicationInsights()
        {

            string applicationInsights = _keyVaultAccess.GetApplicationInsightsConnectionString();
            ApplicationInsightsServiceOptions applicationInsightsServiceOptions = new ApplicationInsightsServiceOptions() { ConnectionString = applicationInsights };

            _services.AddApplicationInsightsTelemetryWorkerService(applicationInsightsServiceOptions);
        }

        private void CreateLoggingService()
        {
            Logging logging = _config.GetLoggingConfiguration();
            _services.AddLogging(loggingBuilder => loggingBuilder.AddFilter<Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider>("Category", Microsoft.Extensions.Logging.LogLevel.Information));
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