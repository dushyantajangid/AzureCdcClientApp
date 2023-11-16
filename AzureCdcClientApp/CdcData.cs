using AzureCdcClientApp.Interface;
using AzureCdcClientApp.Model;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;

namespace AzureCdcClientApp
{
    public class CdcData
    {
        private readonly ISqlTextQuery _sqlTextQuery;
        private readonly IConfigurationSections _config;
        private readonly ILogger<CdcData> _logger;
        private readonly TelemetryClient _telemetryClient;
        private readonly string _sqlDbConnectionString;

        public CdcData(IKeyVaultAccess keyVaultAccess, ISqlTextQuery sqlTextQuery, IConfigurationSections config, TelemetryClient telemetryClient, ILogger<CdcData> logger)
        {
            _sqlTextQuery = sqlTextQuery;
            _config = config;
            _telemetryClient = telemetryClient;
            _logger = logger;
            _sqlDbConnectionString = keyVaultAccess.GetSourceDbConnection();
        }

        public IEnumerable<Dictionary<string, string>> GetCdcData(CdcConfigurationModel cdcConfigurationModel)
        {
            using (_telemetryClient.StartOperation<RequestTelemetry>("CdcData_GetCdcData_operation"))
            {
                _telemetryClient.TrackEvent("CdcData:GetCdcData started");
                IEnumerable<Dictionary<string, string>> queryResult = null;

                cdcConfigurationModel.table_offset = cdcConfigurationModel.table_offset.Trim() == string.Empty ? "0" : cdcConfigurationModel.table_offset;
                string cdcDataTableName = "[cdc].[" + cdcConfigurationModel.table_name.Replace(".", "_") + "_CT]";

                var selectDataQuery = "select '" + cdcConfigurationModel.table_name + "' as table_name, ROW_NUMBER() OVER (ORDER BY [__$start_lsn]) as RowNbr, " +
                    "convert(nvarchar(100), __$start_lsn, 1) as start_lsn_string, convert(nvarchar(100), __$seqval, 1) as seqval_string, [__$operation] as operation," +
                    " convert(nvarchar(100), __$update_mask, 1) as update_mask_string, *" +
                     " from " + cdcDataTableName + " where __$start_lsn > " + cdcConfigurationModel.table_offset + " order by __$start_lsn";

                try
                {
                    _logger.LogDebug("CdcData: GetCdcData Getting CDC data from table: " + cdcDataTableName);
                    queryResult = _sqlTextQuery.PerformQuery(selectDataQuery, _sqlDbConnectionString);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "CdcData:GetCdcData");
                    _logger.LogError("CdcData:GetCdcData: Error occured");
                }
                finally
                {
                    _telemetryClient.TrackEvent("CdcData:GetCdcData completed");
                }

                return queryResult;
            }
        }
    }
}