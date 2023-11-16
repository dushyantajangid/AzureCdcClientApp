using AzureCdcClientApp.Interface;
using Microsoft.Extensions.Logging;

namespace AzureCdcClientApp.ServiceCollections
{
    class LoggingBuilder
    {
        IConfigurationSections _config;
        public LoggingBuilder(IConfigurationSections config)
        {
            _config = config;
        }
        public ILoggerFactory BuildLogger()
        {
            return LoggerFactory.Create(builder =>
            {
                builder.AddConfiguration(_config.GetConfigurationSection("Logging"));
                builder.AddConsole();
            });
        }
    }
}
