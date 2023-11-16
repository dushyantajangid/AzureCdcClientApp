using AzureCdcClientApp.Interface;
using AzureCdcClientApp.Model;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AzureCdcClientApp
{
    public class KeyVaultAccess : IKeyVaultAccess
    {
        private readonly IConfigurationSections _config;
        private readonly KeyVaultOptions _keyVaultOptions;
        private readonly AzureOptions _azureOptions;
        private readonly IKeyVaultClient _keyVaultClient;
        private readonly ILogger<KeyVaultAccess> _logger;

        public KeyVaultAccess(IConfigurationSections config, ILogger<KeyVaultAccess> logger)
        {
            _config = config;
            _logger = logger;
            _keyVaultOptions = _config.GetKeyVaultConfiguration();
            _azureOptions = _config.GetAzureConfiguration();
            _keyVaultClient = GetKeyVaultClient();
        }

        public string GetSourceDbConnection()
        {
            string secret = string.Empty;
            _logger.LogInformation("KeyVaultAccess:GetSourceDbConnection: Started");
            try
            {
                _logger.LogDebug("KeyVaultAccess:GetSourceDbConnection receiving " + _keyVaultOptions.SourceDbKey + " secret from " + _keyVaultOptions.Url);

                Task<SecretBundle> secretBundle = Task.Run(() =>
                _keyVaultClient.GetSecretAsync(_keyVaultOptions.Url, _keyVaultOptions.SourceDbKey));
                secretBundle.Wait();

                secret = secretBundle.Result.Value;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "KeyVaultAccess:GetSourceDbConnection");
                _logger.LogCritical("KeyVaultAccess:GetSourceDbConnection: Error occured, Couldn't get source db connection data");
                throw new Exception("KeyVault connection error");
            }
            _logger.LogInformation("KeyVaultAccess:GetSourceDbConnection: Completed");
            return secret;
        }

        public string GetDestinationDbConnection()
        {
            string secret = string.Empty;
            _logger.LogInformation("KeyVaultAccess:GetDestinationDbConnection: Started");
            try
            {
                _logger.LogDebug("KeyVaultAccess:GetDestinationDbConnection receiving " + _keyVaultOptions.DestinationDbKey + " secret from " + _keyVaultOptions.Url);

                Task<SecretBundle> secretBundle = Task.Run(() =>
                _keyVaultClient.GetSecretAsync(_keyVaultOptions.Url, _keyVaultOptions.DestinationDbKey));
                secretBundle.Wait();

                secret = secretBundle.Result.Value;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "KeyVaultAccess:GetDestinationDbConnection");
                _logger.LogCritical("KeyVaultAccess:GetDestinationDbConnection: Error occured, Couldn't get destination db connection data");
                throw new Exception("KeyVault connection error");
            }
            _logger.LogInformation("KeyVaultAccess:GetDestinationDbConnection: Completed");
            return secret;
        }

        public string GetApplicationInsightsConnectionString()
        {
            string secret = string.Empty;
            _logger.LogInformation("KeyVaultAccess:GetApplicationInsightsConnectionString: Started");
            try
            {
                _logger.LogDebug("KeyVaultAccess:GetApplicationInsightsConnectionString receiving " + _keyVaultOptions.ApplicationInsightsKey + " secret from " + _keyVaultOptions.Url);

                Task<SecretBundle> secretBundle = Task.Run(() =>
                _keyVaultClient.GetSecretAsync(_keyVaultOptions.Url, _keyVaultOptions.ApplicationInsightsKey));
                secretBundle.Wait();

                secret = secretBundle.Result.Value;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "KeyVaultAccess:GetApplicationInsightsConnectionString");
                _logger.LogCritical("KeyVaultAccess:GetApplicationInsightsConnectionString: Error occured, Couldn't get keyvault data");
                throw new Exception("KeyVault connection error");
            }
            _logger.LogInformation("KeyVaultAccess:GetApplicationInsightsConnectionString: Completed");
            return secret;
        }

        private IKeyVaultClient GetKeyVaultClient()
        {
            return new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(
                    async (string auth, string res, string scope) =>
                    {
                        var authContext = new AuthenticationContext(auth);
                        var credential = new ClientCredential(_azureOptions.ApplicationId, _azureOptions.AuthenticationKey);
                        AuthenticationResult result = await authContext.AcquireTokenAsync(res, credential);

                        if (result == null)
                        {
                            throw new InvalidOperationException("Failed to retrieve token");
                        }

                        return result.AccessToken;
                    }
                 ));
        }
    }
}