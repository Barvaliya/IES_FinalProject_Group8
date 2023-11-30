using Microsoft.EntityFrameworkCore;
using AuthorizeAuthenticate.Models;

using System;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Data;
using System.Text;

namespace YourNamespace
{
    public class DatabaseConnector
    {
        private readonly string _connectionString;

        public DatabaseConnector(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MySqlConnection");
        }

        public int GetCount(string query)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            using var cmd = new MySqlCommand(query, connection);
            var count = Convert.ToInt32(cmd.ExecuteScalar());

            return count;
        }

        public void RunQuery(string query)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            using var cmd = new MySqlCommand(query, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                // Access data from the reader if needed
                // Example:
                // var value = reader.GetString("columnName");
            }
        }

        public string ExecuteDirectQuery(string query)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            using var cmd = new MySqlCommand(query, connection);
            var result = cmd.ExecuteScalar();

            return result?.ToString();
        }

        public DataTable GetData(string query)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            using var cmd = new MySqlCommand(query, connection);
            using var adapter = new MySqlDataAdapter(cmd);
            var dataTable = new DataTable();

            adapter.Fill(dataTable);

            return dataTable;
        }

        public List<Dictionary<string, object>> FetchData(string query)
        {
            List<Dictionary<string, object>> resultSet = new List<Dictionary<string, object>>();

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            using var cmd = new MySqlCommand(query, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var rowData = new Dictionary<string, object>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    rowData[reader.GetName(i)] = reader.GetValue(i);
                }

                resultSet.Add(rowData);
            }

            return resultSet;
        }

        public List<Dictionary<string, object>> FetchDataUsingReader(string query)
        {
            List<Dictionary<string, object>> resultSet = new List<Dictionary<string, object>>();

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            using var cmd = new MySqlCommand(query, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var rowData = new Dictionary<string, object>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    rowData[reader.GetName(i)] = reader.GetValue(i);
                }

                resultSet.Add(rowData);
            }

            return resultSet;
        }


    }
}
