using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace MonsterTradingCardsGame.Helpers
{
    public static class DBHelper
    {
        private readonly static string connectionString = "Host=localhost;Username=postgres;Password=123456;Database=mtcg";

        public static bool RegisterUser(string username, string hashedPassword)
        {
            string insertStatement = "INSERT INTO \"user\" (username, password, coins) VALUES (@username, @password, @coins)";
            string userAlreadyExistingStatement = "SELECT * FROM \"user\" WHERE username = @username";

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (var cmd = new NpgsqlCommand(userAlreadyExistingStatement, connection))
                    {
                        cmd.Parameters.AddWithValue("username", username);

                        if (cmd.ExecuteScalar() != null)
                        {
                            return false;
                        }
                        else
                        {
                            using (var inner = new NpgsqlCommand(insertStatement, connection))
                            {
                                inner.Parameters.AddWithValue("username", username);
                                inner.Parameters.AddWithValue("password", hashedPassword);
                                inner.Parameters.AddWithValue("coins", 20);

                                int rowsAffected = inner.ExecuteNonQuery();
                            }

                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Something went wrong in the registration: {ex.Message}");
                return false;
            }

        }

        public static bool LoginUser(string username, string hashedPassword)
        {
            string userEvaluationCheck = "SELECT * FROM \"user\" WHERE username = @username AND password = @password";

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (var command = new NpgsqlCommand(userEvaluationCheck, connection))
                    {
                        command.Parameters.AddWithValue("username", username);
                        command.Parameters.AddWithValue("password", hashedPassword);

                        command.ExecuteScalar();

                        if (command.ExecuteScalar() != null)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Something went wrong while logging in. Message: {ex.Message}");
                return false;
            }
        }
    }
}
