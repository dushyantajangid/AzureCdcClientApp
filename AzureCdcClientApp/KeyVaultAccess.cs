using AzureCdcClientApp.Interface;
using AzureCdcClientApp.Model;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AzureCdcClientApp
{
    public class KeyVaultAccess : IKeyVaultAccess
    {
        private readonly IConfigurationSections _config;
        private readonly KeyVaultOptions _keyVaultOptions;
        private readonly AzureOptions _azureOptions;
        private readonly IKeyVaultClient _keyVaultClient;

        public KeyVaultAccess(IConfigurationSections config)
        {
            _config = config;
            _keyVaultOptions = _config.GetKeyVaultConfiguration();
            _azureOptions = _config.GetAzureConfiguration();
            _keyVaultClient = GetKeyVaultClient();
        }

        public string GetSourceDbConnection()
        {
            string secret = string.Empty;

            try
            {
                Task<SecretBundle> secretBundle = Task.Run(() =>
                _keyVaultClient.GetSecretAsync(_keyVaultOptions.Url, _keyVaultOptions.SourceDbKey));
                secretBundle.Wait();

                secret = secretBundle.Result.Value;
            }
            catch (Exception ex)
            { }

            return secret;
        }

        public string GetDestinationDbConnection()
        {
            string secret = string.Empty;

            try
            {
                Task<SecretBundle> secretBundle = Task.Run(() =>
                _keyVaultClient.GetSecretAsync(_keyVaultOptions.Url, _keyVaultOptions.DestinationDbKey));
                secretBundle.Wait();

                secret = secretBundle.Result.Value;
            }
            catch (Exception ex)
            { }

            return secret;
        }

        public string GetApplicationInsightsConnectionString()
        {
            string secret = string.Empty;

            try
            {
                Task<SecretBundle> secretBundle = Task.Run(() =>
                _keyVaultClient.GetSecretAsync(_keyVaultOptions.Url, _keyVaultOptions.ApplicationInsightsKey));
                secretBundle.Wait();

                secret = secretBundle.Result.Value;
            }
            catch (Exception ex)
            { }

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