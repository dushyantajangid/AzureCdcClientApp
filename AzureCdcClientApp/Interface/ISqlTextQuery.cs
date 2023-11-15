using AzureCdcClientApp.Model;

namespace AzureCdcClientApp.Interface
{
    public interface ISqlTextQuery
    {
        public IEnumerable<Dictionary<string, string>> PerformQuery(string query, string sqlDbConnectionString);
    }
}
