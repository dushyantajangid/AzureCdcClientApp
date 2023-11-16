using AzureCdcClientApp.Extensions;
using AzureCdcClientApp.Interface;
using AzureCdcClientApp.Model;

namespace AzureCdcClientApp
{
    public class PrintOutput : IPrintOutput
    {
        public void WelcomeMessage()
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("=======================================================================================");
            Console.WriteLine("==== CDC Client Application                                                        ====");
            Console.WriteLine("==== Please make sure all configuration is updated in appsettings.json file        ====");
            Console.WriteLine("==== Please make sure all configuration is updated in [dbo].[CdcTableOffset] table ====");
            Console.WriteLine("=======================================================================================");
        }
        public void EndMessage()
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("=======================================================================================");
            Console.WriteLine("= Please enter any value to exit                                                   ====");
            Console.WriteLine("=======================================================================================");
            Console.ReadLine();
        }
        public void CdcConfigurationMissing()
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("=======================================================================================");
            Console.WriteLine("= CDC configuration is missing, Please update [dbo].[CdcTableOffset] table");
            Console.WriteLine("=======================================================================================");
        }
        public void TableProcessedMessage(string tableName)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("=======================================================================================");
            Console.WriteLine("= Table " + tableName + " has been processed");
            Console.WriteLine("=======================================================================================");
        }
        public void AllCdcConfiguration(IEnumerable<Dictionary<string, string>> cdcConfigList)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("=======================================================================================");
            Console.WriteLine("= Available CDC Configuration");
            Console.WriteLine("|| Id|| Table Name    || Table Offset         || Batch Size || Last Checked ||");

            foreach (var configList in cdcConfigList)
            {
                CdcConfigurationModel cdcObj = configList.ToObject<CdcConfigurationModel>();

                Console.WriteLine("|| " + cdcObj.table_id + " || " + cdcObj.table_name + "||" + cdcObj.table_offset + "||" + cdcObj.batch_size + "||" + cdcObj.last_checked);
            }

            Console.WriteLine("=======================================================================================");
        }
    }
}
