using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SRO
{
    public class SQLDriver
    {
        #region Private Members
        private string ConnectionString { get; }
        #endregion

        #region Constructor
        public SQLDriver(string Host, string Username, string Password, string Database)
        {
            ConnectionString = $"Data Source={Host};Initial Catalog={Database};User ID={Username};Password={Password}";
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Check if database connection is available.
        /// </summary>
        public bool CanConnect()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    return connection.State == ConnectionState.Open;
                }
                catch (SqlException ex)
                {
                    // Catches invalid login, wrong server, wrong database name, etc.
                    System.Diagnostics.Debug.WriteLine($"Database exception: {ex.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    // Catches unexpected runtime errors
                    System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                    return false;
                }
            }
        }
        /// <summary>
        /// Executes a query, returns their results.
        /// </summary>
        public List<Dictionary<string, object>> ExecuteQuery(string query, KeyValuePair<string, object>[] bindingParams = null)
        {
            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(query, conn))
            {
                conn.Open();

                // Binding params
                if (bindingParams != null)
                    foreach (var param in bindingParams)
                        cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);

                using (var reader = cmd.ExecuteReader())
                {
                    // Cache column names
                    string[] columnNames = new string[reader.FieldCount];
                    for (int i = 0; i < reader.FieldCount; i++)
                        columnNames[i] = reader.GetName(i);

                    // Rows
                    var rows = new List<Dictionary<string, object>>();

                    while (reader.Read())
                    {
                        var row = new Dictionary<string, object>();
                        // Map column by name
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var value = reader.GetValue(i);
                            row[columnNames[i]] = (value == DBNull.Value) ? null : value;
                        }

                        rows.Add(row);
                    }
                    // Return result
                    return rows;
                }
            }
        }
        #endregion
    }
}
