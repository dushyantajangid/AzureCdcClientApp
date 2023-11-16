
namespace AzureCdcClientApp.Interface
{
    internal interface IPrintOutput
    {
        public void WelcomeMessage();
        public void EndMessage();
        public void CdcConfigurationMissing();
        public void TableProcessedMessage(string tableName);
        public void AllCdcConfiguration(IEnumerable<Dictionary<string, string>> cdcConfigList);
    }
}
