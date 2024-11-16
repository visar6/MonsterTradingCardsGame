using Npgsql;
using System.Reflection;
using System.Text.Json;
using MonsterTradingCardsGame.Helpers;


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
        }

        public abstract bool Handle(HttpServerEventArgs e);
    }
}