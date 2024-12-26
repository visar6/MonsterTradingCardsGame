using MonsterTradingCardsGame.Helpers;
using MonsterTradingCardsGameLibrary.Models;
using System.Text.Json;

namespace MonsterTradingCardsGame.HTTP.Handlers
{
    public class PackageHandler : Handler
    {
        public override bool Handle(HttpServerEventArgs e)
        {
            if (e.Path == "/packages" && e.Method == "POST")
            {
                Console.WriteLine($"[{DateTime.Now}] received 'create package' request...");
                try
                {
                    var json = JsonSerializer.Deserialize<Dictionary<string, object>>(e.Payload);
                    if (json == null)
                    {
                        e.Reply(HttpStatusCode.BAD_REQUEST, "Invalid payload");
                        return true;
                    }

                    string? token = json.GetValueOrDefault("Authorization")?.ToString();
                    if (token != "admin-mtcgToken")
                    {
                        e.Reply(HttpStatusCode.FORBIDDEN, "Only admin can create packages");
                        return true;
                    }

                    Card[]? cards = JsonSerializer.Deserialize<Card[]>(json.GetValueOrDefault("Cards")?.ToString());
                    if (cards == null || cards.Length == 0)
                    {
                        e.Reply(HttpStatusCode.BAD_REQUEST, "No cards provided");
                        return true;
                    }

                    if (DatabaseHelper.AnyCardExists(cards))
                    {
                        e.Reply(HttpStatusCode.CONFLICT, "At least one card already exists");
                        return true;
                    }

                    if (DatabaseHelper.CreatePackage(cards))
                    {
                        e.Reply(HttpStatusCode.CREATED);
                        Console.WriteLine($"[{DateTime.Now}] Package created successfully");
                    }
                    else
                    {
                        e.Reply(HttpStatusCode.BAD_REQUEST, "Failed to create package");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    e.Reply(HttpStatusCode.BAD_REQUEST, "An error occurred");
                }
                return true;
            }

            return false; // Dieser Handler ist nicht zuständig
        }
    }
}
