using MonsterTradingCardsGame.Helpers;
using System.Text.Json;

namespace MonsterTradingCardsGame.HTTP.Handlers
{
    public class UserHandler : Handler
    {
        public override bool Handle(HttpServerEventArgs e)
        {
            if (e.Path == "/users" && e.Method == "POST")
            {
                Console.WriteLine($"[{DateTime.Now}] received 'register' request...");
                try
                {
                    var json = JsonSerializer.Deserialize<Dictionary<string, string>>(e.Payload);
                    if (json == null || string.IsNullOrEmpty(json.GetValueOrDefault("Username")) || string.IsNullOrEmpty(json.GetValueOrDefault("Password")))
                    {
                        e.Reply(HttpStatusCode.BAD_REQUEST, "Invalid username or password");
                        return true;
                    }

                    string username = json["Username"];
                    string password = json["Password"];

                    if (DatabaseHelper.Register(username, password))
                    {
                        e.Reply(HttpStatusCode.CREATED);
                        Console.WriteLine($"[{DateTime.Now}] User '{username}' registered successfully");
                    }
                    else
                    {
                        e.Reply(HttpStatusCode.BAD_REQUEST, "HTTP 400 - User already exists");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    e.Reply(HttpStatusCode.BAD_REQUEST, "An error occurred");
                }
                return true;
            }
            else if (e.Path.StartsWith("/users/") && e.Method == "GET")
            {
                string username = e.Path.Split('/')[2];
                var user = DatabaseHelper.GetUser(username);

                if (user != null)
                {
                    e.Reply(HttpStatusCode.OK, JsonSerializer.Serialize(user));
                }
                else
                {
                    e.Reply(HttpStatusCode.NOT_FOUND, "User not found");
                }
                return true;
            }

            return false; // Dieser Handler ist nicht zuständig
        }
    }
}
