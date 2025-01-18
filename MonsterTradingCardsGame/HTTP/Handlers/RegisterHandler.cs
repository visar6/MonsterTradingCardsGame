using MonsterTradingCardsGame.Helpers;
using System.Text.Json;

namespace MonsterTradingCardsGame.HTTP.Handlers
{
    public class RegisterHandler : Handler
    {
        public override bool Handle(HttpServerEventArgs e)
        {
            if (e.Path == "/users" && e.Method == "POST")
            {
                Console.WriteLine($"[{DateTime.Now}] Received 'register' request...");
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
                        HandlerHelper.PrintSuccess($"[{DateTime.Now}] User '{username}' registered successfully");
                    }
                    else
                    {
                        e.Reply(HttpStatusCode.BAD_REQUEST, "User already exists");
                        HandlerHelper.PrintError($"[{DateTime.Now}] User '{username}' already exists");
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
                Console.WriteLine($"[{DateTime.Now}] Received 'GET user' request...");

                string username = e.Path.Split('/')[2];
                var user = DatabaseHelper.GetUser(username);

                if (user != null)
                {
                    e.Reply(HttpStatusCode.OK, JsonSerializer.Serialize(user));
                    HandlerHelper.PrintSuccess($"[{DateTime.Now}] User '{username}' found and data sent.");
                }
                else
                {
                    e.Reply(HttpStatusCode.NOT_FOUND);
                    HandlerHelper.PrintError($"[{DateTime.Now}] User '{username}' not found.");
                }
                return true;
            }
            else if (e.Path.StartsWith("/users/") && e.Method == "PUT")
            {
                Console.WriteLine($"[{DateTime.Now}] Received 'edit user' request...");

                try
                {
                    var token = e.Headers.FirstOrDefault(h => h.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                        ?.Value.Split(" ")[1];

                    if (string.IsNullOrEmpty(token) || !DatabaseHelper.IsValidToken(token))
                    {
                        e.Reply(HttpStatusCode.UNAUTHORIZED, "Invalid or missing token");
                        return true;
                    }

                    var authenticatedUser = DatabaseHelper.GetUsernameFromToken(token);

                    string username = e.Path.Split('/')[2];

                    if (authenticatedUser != username)
                    {
                        e.Reply(HttpStatusCode.FORBIDDEN);
                        HandlerHelper.PrintError($"[{DateTime.Now}] User '{username}' not allowed to edit this user due to false authentication");

                        return true;
                    }

                    var json = JsonSerializer.Deserialize<Dictionary<string, string>>(e.Payload);
                    if (json == null || !json.ContainsKey("Name") || !json.ContainsKey("Bio") || !json.ContainsKey("Image"))
                    {
                        e.Reply(HttpStatusCode.BAD_REQUEST, "Missing required fields: Name, Bio, or Image");
                        return true;
                    }

                    if (DatabaseHelper.UpdateUser(username, json["Name"], json["Bio"], json["Image"]))
                    {
                        e.Reply(HttpStatusCode.OK);
                        HandlerHelper.PrintSuccess($"[{DateTime.Now}] User '{username}' updated their profile");
                    }
                    else
                    {
                        e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR, "Failed to update user data");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR, "An error occurred");
                }

                return true;
            }

            return false; 
        }
    }
}
