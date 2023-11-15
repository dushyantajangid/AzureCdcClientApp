using AzureCdcClientApp.Interface;
using System.Data.SqlClient;

namespace AzureCdcClientApp
{
    public class SqlTextQuery : ISqlTextQuery
    {
        public IEnumerable<Dictionary<string, string>> PerformQuery(string query, string sqlDbConnectionString)
        {
            var command = new SqlCommand(query);

            IEnumerable<Dictionary<string, string>> results = null;

            using (var sqlConnection = new SqlConnection(sqlDbConnectionString))
            {
                sqlConnection.Open();

                command.Connection = sqlConnection;

                using (SqlDataReader r = command.ExecuteReader())
                {
                    results = Serialize(r);
                }
                sqlConnection.Close();
            }

            return results;

        }
        private IEnumerable<Dictionary<string, string>> Serialize(SqlDataReader reader)
        {
            var results = new List<Dictionary<string, string>>();

            while (reader.Read())
            {
                var row = new Dictionary<string, string>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row.Add(reader.GetName(i), reader.GetValue(i).ToString());
                }

                results.Add(row);
            }
            return results;
        }

    }
}