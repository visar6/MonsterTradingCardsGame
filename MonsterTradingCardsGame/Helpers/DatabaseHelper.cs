using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MonsterTradingCardsGameLibrary.Models;

namespace MonsterTradingCardsGame.Helpers
{
    public static class DatabaseHelper
    {
        private readonly static string connectionString = "Host=localhost;Username=postgres;Password=123456;Database=mtcg";

        static NpgsqlConnection GetConnection()
        {
            var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        public static bool Register(string username, string password)
        {
            const string checkUserExistsQuery = "SELECT 1 FROM \"user\" WHERE username = @username";
            const string insertUserQuery = "INSERT INTO \"user\" (username, password, coins) VALUES (@username, @password, @coins)";

            try
            {
                using (var connection = GetConnection())
                {
                    using (var checkUserCommand = new NpgsqlCommand(checkUserExistsQuery, connection))
                    {
                        checkUserCommand.Parameters.AddWithValue("username", username);

                        if (checkUserCommand.ExecuteScalar() != null)
                        {
                            return false;
                        }
                    }

                    using (var insertUserCommand = new NpgsqlCommand(insertUserQuery, connection))
                    {
                        insertUserCommand.Parameters.AddWithValue("username", username);
                        insertUserCommand.Parameters.AddWithValue("password", password);
                        insertUserCommand.Parameters.AddWithValue("coins", 20);

                        insertUserCommand.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Something went wrong in the registration: {ex.Message}");
                return false;
            }

        }

        public static bool Login(string username, string enteredPassword)
        {
            const string userCheckQuery = "SELECT 1 FROM \"user\" WHERE username = @username AND password = @password";

            try
            {
                using (var connection = GetConnection())
                {
                    using (var checkUserCommand = new NpgsqlCommand(userCheckQuery, connection))
                    {
                        checkUserCommand.Parameters.AddWithValue("username", username);
                        checkUserCommand.Parameters.AddWithValue("password", enteredPassword);

                        var commandResult = checkUserCommand.ExecuteScalar();

                        return commandResult != null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Something went wrong while logging in. Message: {ex.Message}");
                return false;
            }
        }

        internal static bool AnyCardExists(Card[] cards)
        {
            throw new NotImplementedException();
        }

        internal static bool CreatePackage(Card[] cards)
        {
            throw new NotImplementedException();
        }

        internal static object GetUser(string selectedUsername)
        {
            throw new NotImplementedException();
        }
    }
}