
namespace AzureCdcClientApp.Model
{
    public class KeyVaultOptions
    {
        public string Url { get; set; }
        public string SourceDbKey { get; set; }
        public string DestinationDbKey { get; set; }
        public string ApplicationInsightsKey { get; set; }
    }
}
