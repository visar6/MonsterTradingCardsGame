using Npgsql;
using System.Reflection;
using System.Text.Json;
using MonsterTradingCardsGame.Helpers;


namespace MonsterTradingCardsGame.HTTP
{
    public abstract class Handler : IHandler
    {
        private static List<IHandler>? _Handlers = null;

        private static List<IHandler> _GetHandlers()
        {
            List<IHandler> rval = new();

            foreach (Type i in Assembly.GetExecutingAssembly().GetTypes()
                              .Where(m => m.IsAssignableTo(typeof(IHandler)) && (!m.IsAbstract)))
            {
                IHandler? h = (IHandler?)Activator.CreateInstance(i);
                if (h != null)
                {
                    rval.Add(h);
                }
            }

            return rval;
        }

        public static void HandleEvent(HttpServerEventArgs e)
        {
            _Handlers ??= _GetHandlers();

            foreach (IHandler i in _Handlers)
            {
                if (i.Handle(e)) return;
            }

            if (e.Path == "/users")
            {
                Console.WriteLine($"[{DateTime.Now}] received 'register' request...");

                try
                {
                    var json = JsonSerializer.Deserialize<Dictionary<string, string>>(e.Payload);

                    if (json == null)
                    {
                        Console.WriteLine($"[{DateTime.Now}] received payload not serializable...");
                        e.Reply(HttpStatusCode.BAD_REQUEST, "ERROR: Received bad request");
                        return;
                    }

                    // Extrahiere die Werte für Username und Password
                    string? username = json?.GetValueOrDefault("Username");
                    string? password = json?.GetValueOrDefault("Password");


                    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                    {
                        Console.WriteLine($"[{DateTime.Now} ERROR: Username or Password null or empty...");
                        e.Reply(HttpStatusCode.BAD_REQUEST, "ERROR: Username or Password null or empty ");
                        return;
                    }

                    Console.WriteLine($"[{DateTime.Now}] received username '{username}' ...");

                    var result = MonsterTradingCardsGame.Helpers.DBHelper.RegisterUser(username, password);

                    if (result)
                    {
                        e.Reply(HttpStatusCode.CREATED);
                        Console.WriteLine($"[{DateTime.Now}] registration for username '{username}' successful ...");
                    }
                    else
                    {
                        e.Reply(HttpStatusCode.BAD_REQUEST);
                        Console.WriteLine($"[{DateTime.Now}] registration for username '{username}' not successful, username already exists ...");
                    }
                }
                catch (JsonException jsonEx)
                {
                    // Fehler bei der Deserialisierung des JSON
                    Console.WriteLine($"Error in deserialization: {jsonEx.Message}");
                    e.Reply(HttpStatusCode.BAD_REQUEST, "Error in processment of request");
                }
                catch (Exception ex)
                {
                    // Allgemeine Fehlerbehandlung
                    Console.WriteLine($"ERROR: {ex.Message}");
                    e.Reply(HttpStatusCode.BAD_REQUEST, "Unknown error");
                }
            }
            else if (e.Path == "/login")
            {
                Console.WriteLine($"[{DateTime.Now}] received 'login' request...");
                
                try
                {
                    var json = JsonSerializer.Deserialize<Dictionary<string, string>>(e.Payload);

                    if (json == null)
                    {
                        Console.WriteLine($"[{DateTime.Now}] received payload not serializable...");
                        e.Reply(HttpStatusCode.BAD_REQUEST, "ERROR: Received bad request");
                        return;
                    }

                    // Extrahiere die Werte für Username und Password
                    string? username = json?.GetValueOrDefault("Username");
                    string? password = json?.GetValueOrDefault("Password");


                    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                    {
                        Console.WriteLine($"[{DateTime.Now} ERROR: Username or Password null or empty...");
                        e.Reply(HttpStatusCode.BAD_REQUEST, "ERROR: Username or Password null or empty ");
                        return;
                    }

                    Console.WriteLine($"[{DateTime.Now}] received username '{username}' ...");

                    var result = MonsterTradingCardsGame.Helpers.DBHelper.LoginUser(username, password);

                    if (result)
                    {
                        e.Reply(HttpStatusCode.OK, "SUCCESS: login successfull");
                        Console.WriteLine($"[{DateTime.Now}] registration for username '{username}' successful ...");
                    }
                    else
                    {
                        e.Reply(HttpStatusCode.BAD_REQUEST, "ERROR: registration not successfull");
                        Console.WriteLine($"[{DateTime.Now}] registration for username '{username}' not successful, username already exists ...");
                    }
                }
                catch(Exception)
                {

                }
            }


            //e.Reply(HttpStatusCode.BAD_REQUEST);
        }

        

        public abstract bool Handle(HttpServerEventArgs e);
    }
}
