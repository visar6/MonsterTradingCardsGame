using MonsterTradingCardsGame.Helpers;
using MonsterTradingCardsGameLibrary.Models;
using System.Collections.Generic;
using System.Linq;

namespace MonsterTradingCardsGame.HTTP.Handlers
{
    public class BattleHandler : Handler
    {
        private static Queue<User> waitingPlayers = new Queue<User>();
        private static List<Battle> activeBattles = new List<Battle>();
        private static readonly object queueLock = new object();

        private readonly IDatabaseHelper databaseHelper;

        public BattleHandler(IDatabaseHelper databaseHelper)
        {
            this.databaseHelper = databaseHelper;
        }
        
        public override bool Handle(HttpServerEventArgs e)
        {
            if (e.Method == "POST" && e.Path == "/battles")
            {
                StartBattle(e);
                return true;
            }

            return false;
        }

        private void StartBattle(HttpServerEventArgs e)
        {
            User? player = GetUserFromRequest(e);

            if (player == null)
            {
                HandlerHelper.PrintError($"[{DateTime.Now}] Battle request denied: Unauthorized user.");
                return;
            }

            HandlerHelper.PrintSuccess($"[{DateTime.Now}] Player '{player.Username}' searches for a battle...");

            lock (queueLock)
            {
                if (waitingPlayers.Count > 0)
                {
                    User opponent = waitingPlayers.Dequeue();

                    HandlerHelper.PrintSuccess($"[{DateTime.Now}] Match found! {player.Username} vs. {opponent.Username}");

                    var battle = new Battle(player.Id, opponent.Id);
                    battle.StartBattle(player, opponent);
                    activeBattles.Add(battle);

                    HandlerHelper.PrintSuccess($"[{DateTime.Now}] Battle started between '{player.Username}' and '{opponent.Username}'!");

                    foreach (var logEntry in battle.Log)
                    {
                        Console.WriteLine($"   {logEntry}");
                    }

                    string? winnerId = battle.WinnerId;

                    if (winnerId == player.Id)
                    {
                        databaseHelper.TransferCardsToWinner(player.Id, opponent.Id);
                        databaseHelper.UpdatePlayerStats(player.Id, true);
                        databaseHelper.UpdatePlayerStats(opponent.Id, false);
                        HandlerHelper.PrintSuccess($"Winner: {player.Username}. Cards transferred from {opponent.Username}.");
                    }
                    else if (winnerId == opponent.Id)
                    {
                        databaseHelper.TransferCardsToWinner(opponent.Id, player.Id);
                        databaseHelper.UpdatePlayerStats(opponent.Id, true);
                        databaseHelper.UpdatePlayerStats(player.Id, false);
                        HandlerHelper.PrintSuccess($"Winner: {opponent.Username}. Cards transferred from {player.Username}.");
                    }
                    else
                    {
                        HandlerHelper.PrintWarning($"Battle between {player.Username} and {opponent.Username} ended in a draw.");
                    }

                    databaseHelper.SaveBattle(battle, winnerId);
                    e.Reply(200, $"Battle finished. Winner: {(winnerId != null ? winnerId : "Draw")}");
                }
                else
                {
                    waitingPlayers.Enqueue(player);
                    HandlerHelper.PrintWarning($"[{DateTime.Now}] {player.Username} is waiting for an opponent...");
                }
            }
        }

        private User? GetUserFromRequest(HttpServerEventArgs e)
        {
            var authHeader = e.Headers.FirstOrDefault(h => h.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase))?.Value;

            if (string.IsNullOrEmpty(authHeader))
            {
                e.Reply(HttpStatusCode.UNAUTHORIZED, "Invalid or missing token");
                HandlerHelper.PrintError($"[{DateTime.Now}] Battle request denied: Missing token.");
                return null;
            }

            var authParts = authHeader.Split(" ");
            if (authParts.Length != 2 || authParts[0] != "Bearer")
            {
                e.Reply(HttpStatusCode.UNAUTHORIZED, "Invalid token format");
                HandlerHelper.PrintError($"[{DateTime.Now}] Battle request denied: Invalid token format.");
                return null;
            }

            string token = authParts[1];

            if (!databaseHelper.IsValidToken(token))
            {
                e.Reply(HttpStatusCode.UNAUTHORIZED, "Invalid token");
                HandlerHelper.PrintError($"[{DateTime.Now}] Battle request denied: Invalid token.");
                return null;
            }

            string? username = databaseHelper.GetUsernameFromToken(token);
            if (string.IsNullOrEmpty(username))
            {
                e.Reply(HttpStatusCode.UNAUTHORIZED, "Unauthorized: Invalid token");
                HandlerHelper.PrintError($"[{DateTime.Now}] Unauthorized battle attempt with invalid token.");
                return null;
            }

            User? user = databaseHelper.GetUserByUsername(username);
            if (user == null)
            {
                e.Reply(HttpStatusCode.UNAUTHORIZED, "Unauthorized: User not found");
                HandlerHelper.PrintError($"[{DateTime.Now}] Unauthorized battle attempt by '{username}', user not found.");
                return null;
            }

            user.Stack.Cards = databaseHelper.GetUserCards(user.Username);
            user.Deck.Cards = databaseHelper.GetUserDeck(user.Username);

            HandlerHelper.PrintSuccess($"[{DateTime.Now}] '{user.Username}' successfully authenticated and joins the battle.");
            return user;
        }
    }
}
