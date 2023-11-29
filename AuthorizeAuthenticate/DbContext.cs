using Microsoft.EntityFrameworkCore;
using AuthorizeAuthenticate.Models;

using System;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

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

            return result?.ToString(); // Return result as a string (or null if result is null)
            return result?.ToString(); // Return result as a string (or null if result is null)
        }
    }
}
