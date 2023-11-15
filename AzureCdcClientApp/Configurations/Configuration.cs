using AzureCdcClientApp.Interface;
using AzureCdcClientApp.Model;
using Microsoft.Extensions.Configuration;

namespace AzureCdcClientApp.Configurations
{
    public class Configuration : IConfigurationSections
    {
        IConfiguration _configuration;

        public Configuration()
        {
            _configuration = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile($"Configurations/appsettings.json", optional: true, reloadOnChange: true)
              .Build();
        }
        public Logging GetLoggingConfiguration()
        {
            var loggingConfig = _configuration.GetSection("Logging");

            return new Logging()
            {
                LogLevel = new LogLevel() { Default = loggingConfig.GetValue<string>("LogLevel:Default") }
            };
        }
        public AzureOptions GetAzureConfiguration()
        {
            var azureOptionsConfig = _configuration.GetSection("AzureOptions");

            return new AzureOptions()
            {
                TenantId = azureOptionsConfig.GetValue<string>("TenantId"),
                ApplicationId = azureOptionsConfig.GetValue<string>("ApplicationId"),
                AuthenticationKey = azureOptionsConfig.GetValue<string>("AuthenticationKey"),
                SubscriptionId = azureOptionsConfig.GetValue<string>("SubscriptionId"),
                ActiveDirectoryAuthority = azureOptionsConfig.GetValue<string>("ActiveDirectoryAuthority"),
                ResourceManagerUrl = azureOptionsConfig.GetValue<string>("ResourceManagerUrl"),
            };
        }
        public KeyVaultOptions GetKeyVaultConfiguration()
        {
            var keyVaultConfig = _configuration.GetSection("KeyVaultOptions");

            return new KeyVaultOptions()
            {
                Url = keyVaultConfig.GetValue<string>("Url"),
                SourceDbKey = keyVaultConfig.GetValue<string>("SourceDbKey"),
                DestinationDbKey = keyVaultConfig.GetValue<string>("DestinationDbKey"),
                ApplicationInsightsKey = keyVaultConfig.GetValue<string>("ApplicationInsightsKey")
            };
        }
    }
}