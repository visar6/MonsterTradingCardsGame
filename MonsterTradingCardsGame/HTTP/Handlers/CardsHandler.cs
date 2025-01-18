using MonsterTradingCardsGame.Helpers;
using MonsterTradingCardsGame.HTTP;
using System.Text.Json;

public class CardsHandler : Handler
{
    private readonly IDatabaseHelper databaseHelper;

    public CardsHandler(IDatabaseHelper databaseHelper)
    {
        this.databaseHelper = databaseHelper;
    }

    public override bool Handle(HttpServerEventArgs e)
    {
        if (e.Path == "/cards" && e.Method == "GET")
        {
            Console.WriteLine($"[{DateTime.Now}] Received 'show all cards' request...");

            try
            {
                var token = e.Headers.FirstOrDefault(h => h.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                    ?.Value.Split(" ")[1];

                if (string.IsNullOrEmpty(token) || !databaseHelper.IsValidToken(token))
                {
                    e.Reply(HttpStatusCode.UNAUTHORIZED, "Unauthorized");
                    HandlerHelper.PrintError($"[{DateTime.Now}] Returned cards for user denied");
                    return true;
                }

                var username = databaseHelper.GetUsernameFromToken(token);
                if (string.IsNullOrEmpty(username))
                {
                    e.Reply(HttpStatusCode.UNAUTHORIZED, "User not found");
                    return true;
                }

                var userCards = databaseHelper.GetUserCards(username);

                var response = JsonSerializer.Serialize(userCards);
                e.Reply(HttpStatusCode.OK, response);
                HandlerHelper.PrintSuccess($"[{DateTime.Now}] Returned cards for user '{username}'");
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
}