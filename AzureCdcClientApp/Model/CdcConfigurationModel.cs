
namespace AzureCdcClientApp.Model
{
    public class CdcConfigurationModel
    {
        public string table_id { get; set; }
        public string table_name { get; set; }
        public string table_offset { get; set; }
        public string batch_size { get; set; }
        public string last_checked { get; set; }
    }
}
