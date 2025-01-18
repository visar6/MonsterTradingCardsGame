using MonsterTradingCardsGame.Helpers;
using MonsterTradingCardsGame.HTTP;
using MonsterTradingCardsGameLibrary.Enums;
using MonsterTradingCardsGameLibrary.Models;
using System.Text.Json;

public class PackageHandler : Handler
{
    private readonly IDatabaseHelper databaseHelper;

    public PackageHandler(IDatabaseHelper databaseHelper)
    {
        this.databaseHelper = databaseHelper;
    }

    public override bool Handle(HttpServerEventArgs e)
    {
        if (e.Path == "/packages" && e.Method == "POST")
        {
            Console.WriteLine($"[{DateTime.Now}] Received 'create package' request...");

            try
            {
                var token = e.Headers.FirstOrDefault(h => h.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase))?.Value;

                if (token != "Bearer admin-mtcgToken")
                {
                    e.Reply(HttpStatusCode.FORBIDDEN);
                    return true;
                }

                var cards = JsonSerializer.Deserialize<List<Card>>(e.Payload);
                if (cards == null || cards.Count == 0)
                {
                    e.Reply(HttpStatusCode.BAD_REQUEST);
                    return true;
                }

                foreach (var card in cards)
                {
                    if (card.Name.Contains("Spell"))
                    {
                        card.CardType = "Spell";
                    }
                    else
                    {
                        card.CardType = "Monster";
                    }

                    if (card.Name.Contains("Water"))
                    {
                        card.ElementType = ElementType.Water;
                    }
                    else if (card.Name.Contains("Fire"))
                    {
                        card.ElementType = ElementType.Fire;
                    }
                    else
                    {
                        card.ElementType = ElementType.Normal;
                    }
                }

                if (cards.GroupBy(c => c.Id).Any(g => g.Count() > 1))
                {
                    e.Reply(HttpStatusCode.BAD_REQUEST);
                    return true;
                }

                if (databaseHelper.CreatePackage(cards))
                {
                    e.Reply(HttpStatusCode.CREATED);
                    HandlerHelper.PrintSuccess($"[{DateTime.Now}] Package created successfully");
                }
                else
                {
                    e.Reply(HttpStatusCode.BAD_REQUEST);
                    HandlerHelper.PrintError($"[{DateTime.Now}] Failed to create package");
                }
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"JSON Error: {jsonEx.Message}");
                e.Reply(HttpStatusCode.BAD_REQUEST);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR);
            }

            return true;
        }

        return false;
    }
}
