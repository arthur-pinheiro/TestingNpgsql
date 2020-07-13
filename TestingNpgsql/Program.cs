using System;
using System.Data;
using Npgsql;

namespace TestNpgsql
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Host=localhost;Database=desenvolvimento;Port=5432;User ID=postgres;Password=postgres;Timeout=120;CommandTimeout=480;";

            using (NpgsqlConnection npgsqlConnection = new NpgsqlConnection(connectionString))
            {
                npgsqlConnection
                    .Open();

                // Creates test table
                string createTestTableQuery = $@"create table if not exists testing_bugged_value ( id serial, testing_column numeric );";
                using (NpgsqlCommand pgsqlcommand = new NpgsqlCommand(createTestTableQuery, npgsqlConnection))
                {
                    pgsqlcommand
                        .ExecuteNonQuery();
                }

                // valorminuto = 0.5126666666666666666666666667 (this variable is supported by c#, and has 28 decimal digits)
                decimal bugged_value = (decimal)30.76 / 60;
                string insertQuery = $@"insert into testing_bugged_value (testing_column) VALUES (@bugged_value)";
                using (NpgsqlCommand pgsqlcommand = new NpgsqlCommand(insertQuery, npgsqlConnection))
                {
                    pgsqlcommand.Parameters
                        .AddWithValue("bugged_value", bugged_value);

                    pgsqlcommand
                        .ExecuteNonQuery();
                }

                // Tries to read the last inserted value (0.5126666666666666666666666667)
                string getDataQuery = "select * from testing_bugged_value order by id desc limit 1;";

                using (NpgsqlCommand npgsqlCommand = new NpgsqlCommand(getDataQuery, npgsqlConnection))
                {
                    using (NpgsqlDataReader npgsqlDataReader = npgsqlCommand.ExecuteReader())
                    {
                        while (npgsqlDataReader.Read())
                            Console.WriteLine("id: {0}\ttesting_bugged_value: {1}\n", npgsqlDataReader[0], npgsqlDataReader[1]);
                    }
                }
                
                npgsqlConnection
                    .Close();
            }
        }
    }
}
