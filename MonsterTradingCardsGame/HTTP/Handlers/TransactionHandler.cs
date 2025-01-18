using MonsterTradingCardsGame.Helpers;
using MonsterTradingCardsGame.HTTP;
using MonsterTradingCardsGameLibrary.Models;
using System.Text.Json;

public class TransactionHandler : Handler
{
    private readonly IDatabaseHelper databaseHelper;

    public TransactionHandler(IDatabaseHelper databaseHelper)
    {
        this.databaseHelper = databaseHelper;
    }

    public override bool Handle(HttpServerEventArgs e)
    {
        if (e.Path == "/transactions/packages" && e.Method == "POST")
        {
            Console.WriteLine($"[{DateTime.Now}] Received 'acquire package' request...");

            try
            {
                var token = e.Headers.FirstOrDefault(h => h.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase))?.Value.Split(" ")[1];

                if (string.IsNullOrEmpty(token) || !databaseHelper.IsValidToken(token))
                {
                    e.Reply(HttpStatusCode.UNAUTHORIZED);
                    return true;
                }

                var username = databaseHelper.GetUsernameFromToken(token);
                if (string.IsNullOrEmpty(username))
                {
                    e.Reply(HttpStatusCode.UNAUTHORIZED, "User not found");
                    return true;
                }

                var user = databaseHelper.GetUser(username);
                if (user == null)
                {
                    e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR);
                    return true;
                }

                const int packagePrice = 5;
                if (user.Coins < packagePrice)
                {
                    e.Reply(HttpStatusCode.FORBIDDEN, "Not enough money");
                    HandlerHelper.PrintError($"[{DateTime.Now}] User '{username}' could not acquire the package due to insufficient coins.");
                    return true;
                }

                var package = databaseHelper.GetNextAvailablePackage();
                if (package == null)
                {
                    e.Reply(HttpStatusCode.NOT_FOUND, "No packages available");
                    HandlerHelper.PrintError($"[{DateTime.Now}] User '{username}' could not acquire the package due to no packages available.");

                    return true;
                }

                var success = databaseHelper.AddCardsToUserStack(user.Id, package.Cards);
                if (!success)
                {
                    e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR, "Failed to add cards to user stack");
                    return true;
                }

                databaseHelper.DeletePackage(package.Id);
                databaseHelper.UpdateUserCoins(user.Id, user.Coins - packagePrice);

                e.Reply(HttpStatusCode.CREATED);
                HandlerHelper.PrintSuccess($"[{DateTime.Now}] User '{username}' acquired package with ID '{package.Id}'");
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
