using AzureCdcClientApp.Model;
using Microsoft.Extensions.Configuration;

namespace AzureCdcClientApp.Interface
{
    public interface IConfigurationSections
    {
        public AzureOptions GetAzureConfiguration();
        public KeyVaultOptions GetKeyVaultConfiguration();
        public IConfigurationSection GetConfigurationSection(string sectionName);
    }
}
