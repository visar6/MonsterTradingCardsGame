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
                return;
            }

            Console.WriteLine($"[{DateTime.Now}] Received 'start battle' request from {player.Username}...");

            lock (queueLock) 
            {
                if (waitingPlayers.Count > 0)
                {
                    User opponent = waitingPlayers.Dequeue();

                    var battle = new Battle(player.Id, opponent.Id);
                    battle.StartBattle(player, opponent);
                    activeBattles.Add(battle);

                    Console.WriteLine($"Battle Log for {battle.Id}:\n" + string.Join("\n", battle.Log));

                    string? winnerId = battle.WinnerId;

                    if (winnerId == player.Id)
                    {
                        DatabaseHelper.TransferCardsToWinner(player.Id, opponent.Id);
                        DatabaseHelper.UpdatePlayerStats(player.Id, true);
                        DatabaseHelper.UpdatePlayerStats(opponent.Id, false);
                    }
                    else if (winnerId == opponent.Id)
                    {
                        DatabaseHelper.TransferCardsToWinner(opponent.Id, player.Id);
                        DatabaseHelper.UpdatePlayerStats(opponent.Id, true);
                        DatabaseHelper.UpdatePlayerStats(player.Id, false);
                    }

                    DatabaseHelper.SaveBattle(battle, winnerId);

                    e.Reply(200, $"Battle finished. Winner: {(winnerId != null ? winnerId : "Draw")}");
                }
                else
                {
                    waitingPlayers.Enqueue(player);
                }
            }
        }

        private User? GetUserFromRequest(HttpServerEventArgs e)
        {
            var authHeader = e.Headers.FirstOrDefault(h => h.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase))?.Value;

            if (string.IsNullOrEmpty(authHeader))
            {
                e.Reply(HttpStatusCode.UNAUTHORIZED, "Invalid or missing token");
                return null;
            }

            var authParts = authHeader.Split(" ");
            if (authParts.Length != 2 || authParts[0] != "Bearer")
            {
                e.Reply(HttpStatusCode.UNAUTHORIZED, "Invalid token format");
                return null;
            }

            string token = authParts[1];

            if (!DatabaseHelper.IsValidToken(token))
            {
                e.Reply(HttpStatusCode.UNAUTHORIZED, "Invalid token");
                return null;
            }

            string? username = DatabaseHelper.GetUsernameFromToken(token);
            if (string.IsNullOrEmpty(username))
            {
                e.Reply(HttpStatusCode.UNAUTHORIZED, "Unauthorized: Invalid token");
                return null;
            }

            User? user = DatabaseHelper.GetUserByUsername(username);
            if (user == null)
            {
                e.Reply(HttpStatusCode.UNAUTHORIZED, "Unauthorized: User not found");
                return null;
            }

            user.Stack.Cards = DatabaseHelper.GetUserCards(user.Username);
            user.Deck.Cards = DatabaseHelper.GetUserDeck(user.Username);

            return user;
        }
    }
}
