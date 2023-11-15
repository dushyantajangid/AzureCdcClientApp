using AzureCdcClientApp.Model;

namespace AzureCdcClientApp.Interface
{
    public interface IConfigurationSections
    {
        public Logging GetLoggingConfiguration();
        public AzureOptions GetAzureConfiguration();
        public KeyVaultOptions GetKeyVaultConfiguration();
    }
}
