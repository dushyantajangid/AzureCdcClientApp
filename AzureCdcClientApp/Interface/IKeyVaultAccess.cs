using AzureCdcClientApp.Model;

namespace AzureCdcClientApp.Interface
{
    public interface IKeyVaultAccess
    {
        public string GetSourceDbConnection();
        public string GetDestinationDbConnection();
        public string GetApplicationInsightsConnectionString();
    }
}
