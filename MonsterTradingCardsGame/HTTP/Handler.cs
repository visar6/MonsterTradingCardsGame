using Npgsql;
using System.Reflection;
using System.Text.Json;
using MonsterTradingCardsGame.Helpers;
using MonsterTradingCardsGameLibrary.Models;

namespace MonsterTradingCardsGame.HTTP
{
    public abstract class Handler : IHandler
    {
        private static List<IHandler>? handlers = null;

        private static List<IHandler> GetHandlers()
        {
            List<IHandler> handlers = new();

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes()
                              .Where(t => t.IsAssignableTo(typeof(IHandler)) && (!t.IsAbstract)))
            {
                IHandler? handler = (IHandler?)Activator.CreateInstance(type);

                if (handlers != null)
                {
                    handlers.Add(handler);
                }
            }

            return handlers;
        }

        public static void HandleEvent(HttpServerEventArgs e)
        {
            handlers ??= GetHandlers();

            foreach (IHandler handler in handlers)
            {
                if (handler.Handle(e)) return;
            }

            if (e.Path == "/users")
            {
                Console.WriteLine($"[{DateTime.Now}] received 'register' request...");

                try
                {
                    var json = JsonSerializer.Deserialize<Dictionary<string, string>>(e.Payload);

                    if (json == null)
                    {
                        Console.WriteLine($"[{DateTime.Now}] ERROR: received payload not serializable...");
                        e.Reply(HttpStatusCode.BAD_REQUEST, "Received bad request");
                        return;
                    }

                    string? username = json?.GetValueOrDefault("Username");
                    string? password = json?.GetValueOrDefault("Password");


                    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                    {
                        Console.WriteLine($"[{DateTime.Now} ERROR: Username or Password null or empty...");
                        e.Reply(HttpStatusCode.BAD_REQUEST, "Username or Password null or empty");
                        return;
                    }

                    Console.WriteLine($"[{DateTime.Now}] received username '{username}' ...");

                    var result = MonsterTradingCardsGame.Helpers.DatabaseHelper.Register(username, password);

                    if (result)
                    {
                        e.Reply(HttpStatusCode.CREATED);
                        Console.WriteLine($"[{DateTime.Now}] registration for username '{username}' successful ...\n");
                    }
                    else
                    {
                        e.Reply(HttpStatusCode.BAD_REQUEST, "HTTP 400 - User already exists");
                        Console.WriteLine($"[{DateTime.Now}] registration for username '{username}' not successful, username already exists ...\n");
                    }
                }
                catch (JsonException jsonEx)
                {
                    Console.WriteLine($"Error in deserialization: {jsonEx.Message}");
                    e.Reply(HttpStatusCode.BAD_REQUEST, "Error in processment of request");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    e.Reply(HttpStatusCode.BAD_REQUEST, "Unknown error");
                }
            }
            else if (e.Path == "/users/{username})" && e.Method == "GET")
            {
                Console.WriteLine($"[{DateTime.Now}] received 'get user data' request...");

                string selectedUsername = e.Path.Split("/")[2];

                if (string.IsNullOrEmpty(selectedUsername))
                {
                    Console.WriteLine($"[{DateTime.Now}] ERROR: Username null or empty...");
                    e.Reply(HttpStatusCode.BAD_REQUEST, "Username null or empty");
                    return;
                }

                Console.WriteLine($"[{DateTime.Now}] received username '{selectedUsername}' ...");

                var user = MonsterTradingCardsGame.Helpers.DatabaseHelper.GetUser(selectedUsername);

                if (user != null) {
                    e.Reply(HttpStatusCode.OK, JsonSerializer.Serialize(user));
                    Console.WriteLine($"[{DateTime.Now}] user data for username '{selectedUsername}' sent ...\n");
                }
                else
                {
                    e.Reply(HttpStatusCode.NOT_FOUND, "HTTP 404 - User not found");
                    Console.WriteLine($"[{DateTime.Now}] ERROR: user data for username '{selectedUsername}' not found ...\n");
                }
            }
            else if (e.Path == "/sessions") 
            {
                Console.WriteLine($"[{DateTime.Now}] received 'login' request...");
                
                try
                {
                    var json = JsonSerializer.Deserialize<Dictionary<string, string>>(e.Payload);

                    if (json == null)
                    {
                        Console.WriteLine($"[{DateTime.Now}] ERROR: received payload not serializable...");
                        e.Reply(HttpStatusCode.BAD_REQUEST, "Bad Request");
                        return;
                    }

                    string? username = json?.GetValueOrDefault("Username");
                    string? password = json?.GetValueOrDefault("Password");


                    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                    {
                        Console.WriteLine($"[{DateTime.Now} ERROR: Username or Password null or empty...");
                        e.Reply(HttpStatusCode.BAD_REQUEST, "Username or Password null or empty");
                        return;
                    }

                    Console.WriteLine($"[{DateTime.Now}] received username '{username}' ...");

                    var result = MonsterTradingCardsGame.Helpers.DatabaseHelper.Login(username, password);

                    if (result)
                    {
                        string token = $"{username}-mtcgToken";

                        e.Reply(HttpStatusCode.OK, token);
                        Console.WriteLine($"[{DateTime.Now}] registration for username '{username}' successful with token '{token}' ...\n");
                    }
                    else
                    {
                        e.Reply(HttpStatusCode.UNAUTHORIZED, "HTTP 401 - Login failed");
                        Console.WriteLine($"[{DateTime.Now}] ERROR: login for username '{username}' not successful, wrong password ...\n");
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    e.Reply(HttpStatusCode.BAD_REQUEST, "Unknown error");
                }
            }
            else if (e.Path == "/packages")
            {
                Console.WriteLine($"[{DateTime.Now}] received 'create package' request...");

                var json = JsonSerializer.Deserialize<Dictionary<string, object>>(e.Payload);

                if (json == null)
                {
                    Console.WriteLine($"[{DateTime.Now}] ERROR: received payload not serializable...");
                    e.Reply(HttpStatusCode.BAD_REQUEST, "Bad Request");
                    return;
                }

                string? token = json.GetValueOrDefault("Authorization")?.ToString();

                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine($"[{DateTime.Now}] ERROR: Missing access token...");
                    e.Reply(HttpStatusCode.UNAUTHORIZED, "HTTP 401 - Access token is missing or invalid");
                    return;
                }

                if (token != "admin-mtcgToken")
                {
                    Console.WriteLine($"[{DateTime.Now}] ERROR: Unauthorized user...");
                    e.Reply(HttpStatusCode.FORBIDDEN, "HTTP 403 - Provided user is not 'admin'");
                    return;
                }
               
                Card[]? cards = JsonSerializer.Deserialize<Card[]>(json.GetValueOrDefault("Cards")?.ToString());

                if (cards == null || cards.Length == 0)
                {
                    Console.WriteLine($"[{DateTime.Now}] ERROR: No cards provided...");
                    e.Reply(HttpStatusCode.BAD_REQUEST, "HTTP 400 - No cards provided");
                    return;
                }

                if (DatabaseHelper.AnyCardExists(cards))
                {
                    Console.WriteLine($"[{DateTime.Now}] ERROR: At least one card already exists...");
                    e.Reply(HttpStatusCode.CONFLICT, "HTTP 409 - At least one card in the package already exists");
                    return;
                }

                if (DatabaseHelper.CreatePackage(cards))
                {
                    e.Reply(HttpStatusCode.CREATED);
                    Console.WriteLine($"[{DateTime.Now}] Package created successfully ...\n");
                }
                else
                {
                    e.Reply(HttpStatusCode.BAD_REQUEST, "HTTP 400 - Package creation failed");
                    Console.WriteLine($"[{DateTime.Now}] ERROR: Package creation failed ...\n");
                }
            }
        }

        public abstract bool Handle(HttpServerEventArgs e);
    }
}