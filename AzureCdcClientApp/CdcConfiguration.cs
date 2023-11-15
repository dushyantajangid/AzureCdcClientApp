using AzureCdcClientApp.Interface;
using AzureCdcClientApp.Model;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;

namespace AzureCdcClientApp
{
    public class CdcConfiguration
    {
        private readonly ISqlTextQuery _sqlTextQuery;
        private readonly IConfigurationSections _config;
        private readonly ILogger<CdcConfiguration> _logger;
        private readonly TelemetryClient _telemetryClient;

        private readonly string _sqlDbConnectionString;

        public CdcConfiguration(IKeyVaultAccess keyVaultAccess, ISqlTextQuery sqlTextQuery, IConfigurationSections config, TelemetryClient telemetryClient, ILogger<CdcConfiguration> logger)
        {
            _sqlTextQuery = sqlTextQuery;
            _config = config;
            _telemetryClient = telemetryClient;
            _logger = logger;
            _sqlDbConnectionString = keyVaultAccess.GetSourceDbConnection();
        }

        public IEnumerable<Dictionary<string, string>> GetCdcConfigurationData()
        {
            using (_telemetryClient.StartOperation<RequestTelemetry>("CdcConfiguration_GetCdcConfigurationData_operation"))
            {
                _telemetryClient.TrackEvent("CdcConfiguration:GetCdcConfigurationData started");
                IEnumerable<Dictionary<string, string>> queryResult = null;

                string cdcTableDetailQuery = "SELECT [table_id], [table_name], [table_offset], [batch_size],[last_checked] FROM [dbo].[CdcTableOffset]";

                try
                {
                    queryResult = _sqlTextQuery.PerformQuery(cdcTableDetailQuery, _sqlDbConnectionString);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "CdcConfiguration:GetCdcConfigurationData: Error occured");
                }
                finally
                {
                    _telemetryClient.TrackEvent("CdcConfiguration:GetCdcConfigurationData completed");
                }

                return queryResult;
            }
        }

        public void UpdateCdcOffsetData(CdcConfigurationModel cdcConfigurationModel, string tableOffset)
        {
            using (_telemetryClient.StartOperation<RequestTelemetry>("CdcConfiguration_UpdateCdcOffsetData_operation"))
            {
                _telemetryClient.TrackEvent("CdcConfiguration:UpdateCdcOffsetData started");
                string cdcTableDetailQuery = "UPDATE [dbo].[CdcTableOffset] SET [table_offset] = '" + tableOffset + "' WHERE [table_id]=" + cdcConfigurationModel.table_id;

                try
                {
                    _sqlTextQuery.PerformQuery(cdcTableDetailQuery, _sqlDbConnectionString);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "CdcConfiguration:UpdateCdcOffsetData: Error occured");
                }
                finally
                {
                    _telemetryClient.TrackEvent("CdcConfiguration:UpdateCdcOffsetData completed");
                }
            }
        }

        public void UpdateCdcLastDateCheckData(CdcConfigurationModel cdcConfigurationModel)
        {
            using (_telemetryClient.StartOperation<RequestTelemetry>("CdcConfiguration_UpdateCdcLastDateCheckData_operation"))
            {
                _telemetryClient.TrackEvent("CdcConfiguration:UpdateCdcLastDateCheckData started");
                string cdcTableDetailQuery = "UPDATE [dbo].[CdcTableOffset] SET [last_checked] = getdate() WHERE [table_id] = " + cdcConfigurationModel.table_id;

                try
                {
                    _sqlTextQuery.PerformQuery(cdcTableDetailQuery, _sqlDbConnectionString);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "CdcConfiguration:UpdateCdcLastDateCheckData: Error occured");
                }
                finally
                {
                    _telemetryClient.TrackEvent("CdcConfiguration:UpdateCdcLastDateCheckData completed");
                }
            }
        }


    }
}