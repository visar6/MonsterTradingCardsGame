using MonsterTradingCardsGame.Helpers;
using System.Text.Json;

namespace MonsterTradingCardsGame.HTTP.Handlers
{
    public class LoginHandler : Handler
    {
        public override bool Handle(HttpServerEventArgs e)
        {
            if (e.Path == "/sessions" && e.Method == "POST")
            {
                Console.WriteLine($"[{DateTime.Now}] received 'login' request...");
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

                    if (DatabaseHelper.Login(username, password))
                    {
                        string token = $"{username}-mtcgToken";
                        e.Reply(HttpStatusCode.OK, token);
                        HandlerHelper.PrintSuccess($"[{DateTime.Now}] User '{username}' logged in successfully");
                    }
                    else
                    {
                        e.Reply(HttpStatusCode.UNAUTHORIZED, "Login failed");
                        HandlerHelper.PrintError($"[{DateTime.Now}] User '{username}' login failed");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    e.Reply(HttpStatusCode.BAD_REQUEST, "An error occurred");
                }

                return true;
            }

            return false;
        }
    }
}
