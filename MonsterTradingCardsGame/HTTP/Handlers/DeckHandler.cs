using MonsterTradingCardsGame.Helpers;
using MonsterTradingCardsGame.HTTP;
using MonsterTradingCardsGameLibrary.Models;
using System.Text.Json;

public class DeckHandler : Handler
{
    public override bool Handle(HttpServerEventArgs e)
    {
        if (e.Path.StartsWith("/deck") && e.Method == "GET")
        {
            Console.WriteLine($"[{DateTime.Now}] Received 'show deck' request...");

            try
            {
                var token = e.Headers.FirstOrDefault(h => h.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                    ?.Value.Split(" ")[1];

                if (string.IsNullOrEmpty(token) || !DatabaseHelper.IsValidToken(token))
                {
                    e.Reply(HttpStatusCode.UNAUTHORIZED, "Invalid or missing token");
                    return true;
                }

                var username = DatabaseHelper.GetUsernameFromToken(token);
                if (string.IsNullOrEmpty(username))
                {
                    e.Reply(HttpStatusCode.UNAUTHORIZED, "User not found");
                    return true;
                }

                var userDeck = DatabaseHelper.GetUserDeck(username);

                if (userDeck == null || userDeck.Count == 0)
                {

                    e.Reply(HttpStatusCode.OK, JsonSerializer.Serialize(new List<Card>()));
                    HandlerHelper.PrintSuccess($"[{DateTime.Now}] Returned deck for user '{username}'");
                    return true;
                }

                var queryParams = ParseQueryParameters(e.Path);

                if (queryParams.TryGetValue("format", out var format) && format.Equals("plain", StringComparison.OrdinalIgnoreCase))
                {
                    var plainTextResponse = string.Join("\n", userDeck.Select(card =>
                        $"{card.Name} - {card.ElementType} - Damage: {card.Damage}"));
                    e.Reply(HttpStatusCode.OK, plainTextResponse);
                }
                else
                {
                    var response = JsonSerializer.Serialize(userDeck);
                    e.Reply(HttpStatusCode.OK, response);
                }

                HandlerHelper.PrintSuccess($"[{DateTime.Now}] Returned deck for user '{username}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR, "An internal error occurred");
            }

            return true;
        }

        if (e.Path == "/deck" && e.Method == "PUT")
        {
            Console.WriteLine($"[{DateTime.Now}] Received 'configure deck' request...");

            try
            {
                var token = e.Headers.FirstOrDefault(h => h.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                    ?.Value.Split(" ")[1];

                if (string.IsNullOrEmpty(token) || !DatabaseHelper.IsValidToken(token))
                {
                    e.Reply(HttpStatusCode.UNAUTHORIZED, "Invalid or missing token");
                    return true;
                }

                var username = DatabaseHelper.GetUsernameFromToken(token);
                if (string.IsNullOrEmpty(username))
                {
                    e.Reply(HttpStatusCode.UNAUTHORIZED, "User not found");
                    return true;
                }

                var cardIds = JsonSerializer.Deserialize<List<string>>(e.Payload);
                if (cardIds == null || cardIds.Count != 4)
                {
                    e.Reply(HttpStatusCode.BAD_REQUEST, "Bad request");
                    HandlerHelper.PrintError($"[{DateTime.Now}] User '{username}' failed to configure a deck. Reason: Invalid card count");
                    return true;
                }

                var userCards = DatabaseHelper.GetUserStack(username);
                if (!cardIds.All(id => userCards.Any(card => card.Id == id)))
                {
                    e.Reply(HttpStatusCode.BAD_REQUEST);
                    HandlerHelper.PrintError($"[{DateTime.Now}] User '{username}' failed. Reason:One or more cards are invalid or not owned by the user");
                    return true;
                }

                if (!DatabaseHelper.UpdateUserDeck(username, cardIds))
                {
                    e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR, "Failed to configure deck");
                    return true;
                }

                e.Reply(HttpStatusCode.OK);
                HandlerHelper.PrintSuccess($"[{DateTime.Now}] User '{username}' configured deck with cards: {string.Join(", ", cardIds)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR, "An internal error occurred");
            }

            return true;
        }



        return false; 
    }

    private Dictionary<string, string> ParseQueryParameters(string path)
    {
        var queryParams = new Dictionary<string, string>();
        var queryStartIndex = path.IndexOf('?');
        if (queryStartIndex != -1)
        {
            var query = path.Substring(queryStartIndex + 1);
            var pairs = query.Split('&');
            foreach (var pair in pairs)
            {
                var kvp = pair.Split('=');
                if (kvp.Length == 2)
                {
                    queryParams[kvp[0]] = Uri.UnescapeDataString(kvp[1]);
                }
            }
        }
        return queryParams;
    }
}


