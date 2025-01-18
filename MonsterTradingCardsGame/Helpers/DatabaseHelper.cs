using Npgsql;
using MonsterTradingCardsGameLibrary.Models;
using MonsterTradingCardsGameLibrary.Enums;
using NpgsqlTypes;
using MonsterTradingCardsGameLibrary.Models.MonsterTradingCardsGameLibrary.Models;
using Microsoft.Extensions.Configuration;
using System.IO;


namespace MonsterTradingCardsGame.Helpers
{
    public class DatabaseHelper : IDatabaseHelper
    {
        private readonly static string connectionString;

        static DatabaseHelper()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            connectionString = config.GetSection("Database:ConnectionString").Value
                               ?? throw new Exception("Database ConnectionString not found in appsettings.json.");
        }

        static NpgsqlConnection GetConnection()
        {
            var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        // Benutzerverwaltung

        public bool Register(string username, string password)
        {
            const string checkUserExistsQuery = "SELECT 1 FROM \"user\" WHERE username = @username";
            const string insertUserQuery = "INSERT INTO \"user\" (id, username, password, coins, token) VALUES (@id, @username, @password, @coins, @token)";
            const string insertStatsQuery = "INSERT INTO Stats (id, userId, elo, wins, losses) VALUES (@id, @userId, 1000, 0, 0)";

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

                    string userId = Guid.NewGuid().ToString();
                    string statsId = Guid.NewGuid().ToString();
                    password = BCrypt.Net.BCrypt.HashPassword(password);

                    using (var insertUserCommand = new NpgsqlCommand(insertUserQuery, connection))
                    {
                        insertUserCommand.Parameters.AddWithValue("id", userId);
                        insertUserCommand.Parameters.AddWithValue("username", username);
                        insertUserCommand.Parameters.AddWithValue("password", password);
                        insertUserCommand.Parameters.AddWithValue("coins", 20);
                        insertUserCommand.Parameters.AddWithValue("token", $"{username}-mtcgToken");

                        insertUserCommand.ExecuteNonQuery();
                    }

                    using (var insertStatsCommand = new NpgsqlCommand(insertStatsQuery, connection))
                    {
                        insertStatsCommand.Parameters.AddWithValue("id", statsId);
                        insertStatsCommand.Parameters.AddWithValue("userId", userId);
                        insertStatsCommand.Parameters.AddWithValue("elo", 1000);
                        insertStatsCommand.Parameters.AddWithValue("wins", 0);
                        insertStatsCommand.Parameters.AddWithValue("losses", 0);

                        insertStatsCommand.ExecuteNonQuery();
                    }

                    return true;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Something went wrong in the registration: {ex.Message}");
                return false;
            }
        }

        public bool Login(string username, string enteredPassword)
        {
            const string getPasswordQuery = "SELECT password FROM \"user\" WHERE username = @username";

            try
            {
                using (var connection = GetConnection())
                {
                    using (var command = new NpgsqlCommand(getPasswordQuery, connection))
                    {
                        command.Parameters.AddWithValue("username", username);

                        var result = command.ExecuteScalar();

                        if (result != null)
                        {
                            string storedHashedPassword = result.ToString();

                            if (BCrypt.Net.BCrypt.Verify(enteredPassword, storedHashedPassword))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Login failed. Message: {ex.Message}");
            }

            return false;
        }

        public User GetUser(string selectedUsername)
        {
            const string getUserQuery = "SELECT * FROM \"user\" WHERE username = @username";

            try
            {
                using (var connection = GetConnection())
                {
                    using (var getUserCommand = new NpgsqlCommand(getUserQuery, connection))
                    {
                        getUserCommand.Parameters.AddWithValue("username", selectedUsername);

                        using (var reader = getUserCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new User
                                {
                                    Id = reader["id"].ToString(),
                                    Username = reader["username"].ToString(),
                                    Coins = Convert.ToInt32(reader["coins"]),
                                    Bio = reader["bio"].ToString(),
                                    Image = reader["image"].ToString(),
                                    Name = reader["name"].ToString()
                                };
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to retrieve user. Message: {ex.Message}");
                return null;
            }
        }

        public User? GetUserByUsername(string username)
        {
            const string query = "SELECT id, username, coins FROM \"user\" WHERE username = @username";

            try
            {
                using (var connection = GetConnection())
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("username", username);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new User
                                {
                                    Id = reader.GetString(reader.GetOrdinal("id")),
                                    Username = reader.GetString(reader.GetOrdinal("username")),
                                    Coins = reader.GetInt32(reader.GetOrdinal("coins"))
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to retrieve user '{username}'. Message: {ex.Message}");
            }

            return null;
        }

        public bool UpdateUser(string username, string name, string bio, string image)
        {
            const string query = "UPDATE \"user\" SET name = @Name, bio = @Bio, image = @Image WHERE username = @Username";

            try
            {
                using (var connection = GetConnection())
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("Name", name);
                        command.Parameters.AddWithValue("Bio", bio);
                        command.Parameters.AddWithValue("Image", image);
                        command.Parameters.AddWithValue("Username", username);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        public string? GetUsernameFromToken(string token)
        {
            const string query = "SELECT username FROM \"user\" WHERE token = @token";

            try
            {
                using (var connection = GetConnection())
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("token", token);
                        var result = command.ExecuteScalar();
                        return result?.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to retrieve username from token. Message: {ex.Message}");
                return null;
            }
        }

        public bool IsValidToken(string token)
        {
            string query = "SELECT COUNT(*) FROM \"user\" WHERE token = @token";

            try
            {
                using (var connection = GetConnection())
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.Add("@token", NpgsqlDbType.Varchar).Value = token;

                        var result = command.ExecuteScalar();

                        return result != null && (long)result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to validate token. Message: {ex.Message}");
                return false;
            }
        }

        // Kartenverwaltung & Deck-Management

        public List<Card> GetUserCards(string username)
        {
            const string query = @"
                                 SELECT c.id, c.name, c.elementType, c.damage, c.cardType
                                 FROM Stack s
                                 INNER JOIN Card c ON s.cardId = c.id
                                 INNER JOIN ""user"" u ON s.userId = u.id
                                 WHERE u.username = @Username";

            try
            {
                using (var connection = GetConnection())
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("username", username);

                        using (var reader = command.ExecuteReader())
                        {
                            var cards = new List<Card>();

                            while (reader.Read())
                            {
                                var card = new Card
                                {
                                    Id = reader.GetString(reader.GetOrdinal("id")),
                                    Name = reader.GetString(reader.GetOrdinal("name")),
                                    ElementType = Enum.TryParse<ElementType>(reader.GetString(reader.GetOrdinal("elementType")), out var elementType)
                                    ? elementType : (ElementType?)null,
                                    CardType = reader.GetString(reader.GetOrdinal("cardType")),
                                    Damage = reader.GetDouble(reader.GetOrdinal("damage"))
                                };
                                cards.Add(card);
                            }

                            return cards;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to retrieve user cards. Message: {ex.Message}");
                return new List<Card>();
            }
        }

        public List<Card> GetUserStack(string username)
        {
            const string query = @"
                                 SELECT c.id, c.name, c.elementType, c.damage, c.cardType
                                 FROM Stack s
                                 INNER JOIN Card c ON s.cardId = c.id
                                 INNER JOIN ""user"" u ON s.userId = u.id
                                 WHERE u.username = @Username";

            try
            {
                using (var connection = GetConnection())
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("Username", username);

                        using (var reader = command.ExecuteReader())
                        {
                            var cards = new List<Card>();

                            while (reader.Read())
                            {
                                cards.Add(new Card
                                {
                                    Id = reader.GetString(reader.GetOrdinal("id")),
                                    Name = reader.GetString(reader.GetOrdinal("name")),
                                    ElementType = Enum.TryParse<ElementType>(reader.GetString(reader.GetOrdinal("elementType")), out var elementType)
                                    ? elementType : (ElementType?)null,
                                    CardType = reader.GetString(reader.GetOrdinal("cardType")),
                                    Damage = reader.GetDouble(reader.GetOrdinal("damage"))
                                });
                            }

                            return cards;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to retrieve user stack. Message: {ex.Message}");
                return new List<Card>();
            }
        }

        public List<Card>? GetUserDeck(string username)
        {
            const string query = @"
                                 SELECT c.id, c.name, c.elementType, c.damage, c.cardType
                                 FROM Deck d
                                 INNER JOIN Card c ON d.cardId = c.id
                                 INNER JOIN ""user"" u ON d.userId = u.id
                                 WHERE u.username = @username";

            try
            {
                using (var connection = GetConnection())
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("username", username);

                        using (var reader = command.ExecuteReader())
                        {
                            var cards = new List<Card>();

                            while (reader.Read())
                            {
                                var card = new Card
                                {
                                    Id = reader.GetString(reader.GetOrdinal("id")),
                                    Name = reader.GetString(reader.GetOrdinal("name")),
                                    ElementType = Enum.TryParse<ElementType>(reader.GetString(reader.GetOrdinal("elementType")), out var elementType)
                                    ? elementType : (ElementType?)null,
                                    CardType = reader.GetString(reader.GetOrdinal("cardType")),
                                    Damage = reader.GetDouble(reader.GetOrdinal("damage"))
                                };
                                cards.Add(card);
                            }

                            return cards;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to retrieve user deck. Message: {ex.Message}");
                return null;
            }
        }

        public bool UpdateUserDeck(string username, List<string> cardIds)
        {
            const string deleteQuery = "DELETE FROM Deck WHERE userId = (SELECT id FROM \"user\" WHERE username = @Username)";
            const string insertQuery = "INSERT INTO Deck (id, userId, cardId) VALUES (@id, (SELECT id FROM \"user\" WHERE username = @Username), @CardId)";

            try
            {
                using (var connection = GetConnection())
                {
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (var deleteCommand = new NpgsqlCommand(deleteQuery, connection, transaction))
                            {
                                deleteCommand.Parameters.AddWithValue("Username", username);
                                deleteCommand.ExecuteNonQuery();
                            }

                            foreach (var cardId in cardIds)
                            {
                                using (var insertCommand = new NpgsqlCommand(insertQuery, connection, transaction))
                                {
                                    var stackId = Guid.NewGuid().ToString();
                                    insertCommand.Parameters.AddWithValue("id", stackId);
                                    insertCommand.Parameters.AddWithValue("Username", username);
                                    insertCommand.Parameters.AddWithValue("CardId", cardId);
                                    insertCommand.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to update user deck. Message: {ex.Message}");
                return false;
            }
        }

        public bool AddCardsToUserStack(string userId, List<Card> cards)
        {
            const string query = "INSERT INTO Stack (id, userId, cardId) VALUES (@id, @userId, @cardId)";

            try
            {
                using (var connection = GetConnection())
                {
                    foreach (var card in cards)
                    {
                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            var stackId = Guid.NewGuid().ToString();

                            command.Parameters.AddWithValue("id", stackId);
                            command.Parameters.AddWithValue("userId", userId);
                            command.Parameters.AddWithValue("cardId", card.Id);

                            command.ExecuteNonQuery();
                        }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to add cards to user stack. Message: {ex.Message}");
                return false;
            }
        }

        // Paket- & Trade-Funktionen

        public bool CreatePackage(List<Card> cards)
        {
            const string insertCardQuery = "INSERT INTO Card (id, name, damage, cardType, elementType) VALUES (@id, @name, @damage, @cardType, @elementType) ON CONFLICT (id) DO NOTHING";
            const string insertPackageQuery = "INSERT INTO Package (id, price) VALUES (@id, 5)";
            const string insertPackageCardQuery = "INSERT INTO Package_contains_Card (packageId, cardId) VALUES (@packageId, @cardId)";

            try
            {
                using (var connection = GetConnection())
                {
                    var packageId = Guid.NewGuid().ToString();

                    using (var insertPackageCommand = new NpgsqlCommand(insertPackageQuery, connection))
                    {
                        insertPackageCommand.Parameters.AddWithValue("id", packageId);
                        insertPackageCommand.ExecuteNonQuery();
                    }

                    foreach (var card in cards)
                    {
                        using (var insertCardCommand = new NpgsqlCommand(insertCardQuery, connection))
                        {
                            insertCardCommand.Parameters.AddWithValue("id", card.Id);
                            insertCardCommand.Parameters.AddWithValue("name", card.Name);
                            insertCardCommand.Parameters.AddWithValue("damage", card.Damage);
                            insertCardCommand.Parameters.AddWithValue("cardType", card.CardType);
                            insertCardCommand.Parameters.AddWithValue("elementType", card.ElementType.ToString());
                            insertCardCommand.ExecuteNonQuery();
                        }

                        using (var insertPackageCardCommand = new NpgsqlCommand(insertPackageCardQuery, connection))
                        {
                            insertPackageCardCommand.Parameters.AddWithValue("packageId", packageId);
                            insertPackageCardCommand.Parameters.AddWithValue("cardId", card.Id);
                            insertPackageCardCommand.ExecuteNonQuery();
                        }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to create package. Message: {ex.Message}");
                return false;
            }
        }

        public Package? GetNextAvailablePackage()
        {
            const string packageQuery = "SELECT id, price FROM Package LIMIT 1";
            const string cardsQuery = @"
                                      SELECT c.id, c.name, c.elementType, c.damage, c.cardType
                                      FROM Package_contains_Card pcc
                                      INNER JOIN Card c ON pcc.cardId = c.id
                                      WHERE pcc.packageId = @packageId";

            try
            {
                using (var connection = GetConnection())
                {
                    using (var command = new NpgsqlCommand(packageQuery, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Package package = new Package
                                {
                                    Id = reader.GetString(reader.GetOrdinal("id")),
                                    Price = reader.GetInt32(reader.GetOrdinal("price")),
                                    Cards = new List<Card>() 
                                };

                                reader.Close(); 
                                using (var cardCommand = new NpgsqlCommand(cardsQuery, connection))
                                {
                                    cardCommand.Parameters.AddWithValue("@packageId", package.Id);

                                    using (var cardReader = cardCommand.ExecuteReader())
                                    {
                                        while (cardReader.Read())
                                        {
                                            var card = new Card
                                            {
                                                Id = cardReader.GetString(cardReader.GetOrdinal("id")),
                                                Name = cardReader.GetString(cardReader.GetOrdinal("name")),
                                                ElementType = Enum.TryParse<ElementType>(cardReader.GetString(cardReader.GetOrdinal("elementType")), out var elementType)
                                                              ? elementType
                                                              : (ElementType?)null,
                                                CardType = cardReader.GetString(cardReader.GetOrdinal("cardType")),
                                                Damage = cardReader.GetDouble(cardReader.GetOrdinal("damage"))
                                            };
                                            package.Cards.Add(card);
                                        }
                                    }
                                }

                                return package;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to retrieve next available package. Message: {ex.Message}");
            }

            return null;
        }

        public List<Card> GetCardsForPackage(string packageId)
        {
            const string query = @"
                                 SELECT c.id, c.name, c.damage, c.elementtype
                                 FROM Card c
                                 INNER JOIN Package_contains_Card pc ON c.id = pc.card_id
                                 WHERE pc.package_id = @PackageId";

            var cards = new List<Card>();

            try
            {
                using (var connection = GetConnection())
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("PackageId", packageId);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cards.Add(new Card
                                {
                                    Id = reader.GetString(reader.GetOrdinal("id")),
                                    Name = reader.GetString(reader.GetOrdinal("name")),
                                    Damage = reader.GetDouble(reader.GetOrdinal("damage")),
                                    ElementType = Enum.TryParse<ElementType>(
                                        reader.GetString(reader.GetOrdinal("elementtype")),
                                        out var elementType) ? elementType : null
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to retrieve cards for package. Message: {ex.Message}");
            }

            return cards;
        }

        public void DeletePackage(string packageId)
        {
            const string deleteCardsQuery = "DELETE FROM Package_Contains_Card WHERE packageId = @PackageId";
            const string deletePackageQuery = "DELETE FROM Package WHERE id = @PackageId";

            try
            {
                using (var connection = GetConnection())
                {
                    using (var deleteCardsCommand = new NpgsqlCommand(deleteCardsQuery, connection))
                    {
                        deleteCardsCommand.Parameters.AddWithValue("packageId", packageId);
                        deleteCardsCommand.ExecuteNonQuery();
                    }

                    using (var deletePackageCommand = new NpgsqlCommand(deletePackageQuery, connection))
                    {
                        deletePackageCommand.Parameters.AddWithValue("packageId", packageId);
                        deletePackageCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to delete package. Message: {ex.Message}");
            }
        }

        public void UpdateUserCoins(string userId, int newCoins)
        {
            const string query = "UPDATE \"user\" SET coins = @NewCoins WHERE id = @userId";

            try
            {
                using (var connection = GetConnection())
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("newCoins", newCoins);
                        command.Parameters.AddWithValue("userId", userId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to update user coins. Message: {ex.Message}");
            }
        }

        // Kampfverwaltung

        public void SaveBattle(Battle battle, string? winnerId)
        {
            const string query = "INSERT INTO battle (id, userId1, userId2, winnerId) VALUES (@id, @userId1, @userId2, @winnerId)";

            try
            {
                using (var connection = GetConnection())
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("id", battle.Id);
                        command.Parameters.AddWithValue("userId1", battle.UserId1);
                        command.Parameters.AddWithValue("userId2", battle.UserId2);
                        command.Parameters.AddWithValue("winnerId", (object?)winnerId ?? DBNull.Value);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to save battle. Message: {ex.Message}");
            }
        }

        public void TransferCardsToWinner(string winnerId, string loserId)
        {
            const string transferQuery = @"
                                         UPDATE Stack 
                                         SET userId = @winnerId 
                                         WHERE userId = @loserId 
                                         AND cardId IN (SELECT cardId FROM Deck WHERE userId = @loserId)";

            try
            {
                using (var connection = GetConnection())
                {
                    using (var command = new NpgsqlCommand(transferQuery, connection))
                    {
                        command.Parameters.AddWithValue("winnerId", winnerId);
                        command.Parameters.AddWithValue("loserId", loserId);

                        int rowsAffected = command.ExecuteNonQuery();

                        Console.WriteLine($"[{DateTime.Now}] Transferred {rowsAffected} cards from {loserId}'s deck to {winnerId}.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to transfer cards. Message: {ex.Message}");
            }
        }


        public void UpdatePlayerStats(string userId, bool isWinner)
        {
            const string checkStatsQuery = "SELECT COUNT(*) FROM Stats WHERE userId = @userId";
            const string insertStatsQuery = "INSERT INTO Stats (id, userId, elo, wins, losses) VALUES (@id, @userId, 1000, 0, 0)";
            string updateQuery = isWinner
                ? "UPDATE Stats SET elo = elo + 10, wins = wins + 1 WHERE userId = @userId"
                : "UPDATE Stats SET elo = GREATEST(elo - 5, 0), losses = losses + 1 WHERE userId = @userId";

            try
            {
                using (var connection = GetConnection())
                {
                    using (var checkCommand = new NpgsqlCommand(checkStatsQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("userId", userId);
                        int count = Convert.ToInt32(checkCommand.ExecuteScalar());

                        if (count == 0)
                        {
                            using (var insertCommand = new NpgsqlCommand(insertStatsQuery, connection))
                            {
                                insertCommand.Parameters.AddWithValue("id", Guid.NewGuid().ToString());
                                insertCommand.Parameters.AddWithValue("userId", userId);
                                insertCommand.Parameters.AddWithValue("elo", 1000);
                                insertCommand.Parameters.AddWithValue("wins", 0);
                                insertCommand.Parameters.AddWithValue("losses", 0);

                                insertCommand.ExecuteNonQuery();
                            }
                        }
                    }

                    using (var command = new NpgsqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("userId", userId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to update player stats. Message: {ex.Message}");
            }
        }

        // Statistiken & Rangliste

        public Stats? GetUserStats(string userId)
        {
            const string query = @"
                                 SELECT id, userId, elo, wins, losses
                                 FROM stats
                                 WHERE userId = @userId";

            try
            {
                using (var connection = GetConnection())
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("userId", userId);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Stats
                                {
                                    Id = reader.GetString(reader.GetOrdinal("id")),
                                    UserId = reader.GetString(reader.GetOrdinal("userId")),
                                    Elo = reader.GetInt32(reader.GetOrdinal("elo")),
                                    Wins = reader.GetInt32(reader.GetOrdinal("wins")),
                                    Losses = reader.GetInt32(reader.GetOrdinal("losses"))
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to retrieve stats for user '{userId}'. Message: {ex.Message}");
            }

            return null; 
        }

        public List<Stats> GetScoreboard()
        {
            const string query = @"
                                 SELECT id, userId, elo, wins, losses
                                 FROM stats
                                 ORDER BY elo DESC, wins DESC
                                 LIMIT 5"; 

            try
            {
                using (var connection = GetConnection())
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            var scoreboard = new List<Stats>();
                            while (reader.Read())
                            {
                                scoreboard.Add(new Stats
                                {
                                    Id = reader.GetString(reader.GetOrdinal("id")),
                                    UserId = reader.GetString(reader.GetOrdinal("userId")),
                                    Elo = reader.GetInt32(reader.GetOrdinal("elo")),
                                    Wins = reader.GetInt32(reader.GetOrdinal("wins")),
                                    Losses = reader.GetInt32(reader.GetOrdinal("losses"))
                                });
                            }
                            return scoreboard;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to retrieve scoreboard. Message: {ex.Message}");
                return new List<Stats>();
            }
        }

        // Handels-System

        public bool CreateTradingDeal(TradingDeal deal)
        {
            const string query = @"
                                 INSERT INTO TradingDeals (tradingDealId, userId, cardId, desiredCardType, minimumDamage)
                                 VALUES (@tradingDealId, @userId, @cardId, @desiredCardType, @minimumDamage)";

            try
            {
                using (var connection = GetConnection())
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("tradingDealId", deal.Id);
                        command.Parameters.AddWithValue("userId", deal.UserId);
                        command.Parameters.AddWithValue("cardId", deal.CardId);
                        command.Parameters.AddWithValue("desiredCardType", HandlerHelper.CapitalizeFirstLetter(deal.DesiredCardType));
                        command.Parameters.AddWithValue("minimumDamage", deal.MinimumDamage);
                        command.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to create trading deal. Message: {ex.Message}");
                return false;
            }
        }

        public bool DeleteTradingDeal(string tradingDealId, string userId)
        {
            const string query = @"
                                 DELETE FROM TradingDeals
                                 WHERE tradingDealId = @tradingDealId AND userId = @userId";

            try
            {
                using (var connection = GetConnection())
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("tradingDealId", tradingDealId);
                        command.Parameters.AddWithValue("userId", userId);
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to delete trading deal. Message: {ex.Message}");
                return false;
            }
        }

        public List<TradingDeal> GetAllTradingDeals()
        {
            const string query = "SELECT * FROM TradingDeals";

            var deals = new List<TradingDeal>();

            try
            {
                using (var connection = GetConnection())
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                deals.Add(new TradingDeal
                                {
                                    Id = reader.GetString(reader.GetOrdinal("tradingDealId")),
                                    UserId = reader.GetString(reader.GetOrdinal("userId")),
                                    CardId = reader.GetString(reader.GetOrdinal("cardId")),
                                    DesiredCardType = reader.GetString(reader.GetOrdinal("desiredCardType")),
                                    MinimumDamage = reader.GetInt32(reader.GetOrdinal("minimumDamage"))
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to retrieve trading deals. Message: {ex.Message}");
            }

            return deals;
        }

        public bool TradingDealExists(string tradingDealId)
        {
            const string query = "SELECT COUNT(*) FROM TradingDeals WHERE tradingDealId = @tradingDealId";

            try
            {
                using (var connection = GetConnection())
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("tradingDealId", tradingDealId);

                        return (long)command.ExecuteScalar() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to check if trading deal exists. Message: {ex.Message}");
                return false;
            }
        }

        public bool UserOwnsTradingDeal(string userId, string tradingDealId)
        {
            const string query = "SELECT COUNT(*) FROM TradingDeals WHERE tradingDealId = @tradingDealId AND userId = @userId";

            try
            {
                using (var connection = GetConnection())
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("tradingDealId", tradingDealId);
                        command.Parameters.AddWithValue("userId", userId);

                        return (long)command.ExecuteScalar() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to check trading deal ownership. Message: {ex.Message}");
                return false;
            }
        }

        public bool UserOwnsCard(string userId, string cardId)
        {
            const string query = "SELECT COUNT(*) FROM Stack WHERE userId = @userId AND cardId = @cardId";

            try
            {
                using (var connection = GetConnection())
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("userId", userId);
                        command.Parameters.AddWithValue("cardId", cardId);

                        return (long)command.ExecuteScalar() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to check card ownership. Message: {ex.Message}");
                return false;
            }
        }

        public bool CardMeetsTradingRequirements(string offeredCardId, TradingDeal deal)
        {
            const string query = "SELECT cardType, damage FROM Card WHERE id = @cardId";

            try
            {
                using (var connection = GetConnection())
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("cardId", offeredCardId);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string cardType = reader.GetString(reader.GetOrdinal("cardType"));
                                int damage = reader.GetInt32(reader.GetOrdinal("damage"));

                                return cardType == deal.DesiredCardType && damage >= deal.MinimumDamage;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to check if card meets trade requirements. Message: {ex.Message}");
            }

            return false;
        }

        public TradingDeal? GetTradingDeal(string tradingDealId)
        {
            const string query = "SELECT * FROM TradingDeals WHERE tradingDealId = @tradingDealId";

            try
            {
                using (var connection = GetConnection())
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("tradingDealId", tradingDealId);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new TradingDeal
                                {
                                    Id = reader.GetString(reader.GetOrdinal("tradingDealId")),
                                    UserId = reader.GetString(reader.GetOrdinal("userId")),
                                    CardId = reader.GetString(reader.GetOrdinal("cardId")),
                                    DesiredCardType = reader.GetString(reader.GetOrdinal("desiredCardType")),
                                    MinimumDamage = reader.GetInt32(reader.GetOrdinal("minimumDamage"))
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to retrieve trading deal. Message: {ex.Message}");
            }

            return null;
        }

        public bool ExecuteTrade(string buyerId, string offeredCardId, TradingDeal deal)
        {
            const string transferQuery = @"
        UPDATE Stack SET userId = @buyerId WHERE cardId = @cardId;
        UPDATE Stack SET userId = @sellerId WHERE cardId = @offeredCardId;
        DELETE FROM TradingDeals WHERE tradingDealId = @tradingDealId";

            try
            {
                using (var connection = GetConnection())
                {
                    using (var command = new NpgsqlCommand(transferQuery, connection))
                    {
                        command.Parameters.AddWithValue("buyerId", buyerId);
                        command.Parameters.AddWithValue("sellerId", deal.UserId);
                        command.Parameters.AddWithValue("cardId", deal.CardId);
                        command.Parameters.AddWithValue("offeredCardId", offeredCardId);
                        command.Parameters.AddWithValue("tradingDealId", deal.Id);

                        command.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to execute trade. Message: {ex.Message}");
                return false;
            }
        }
    }
}