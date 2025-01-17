using MonsterTradingCardsGame.Helpers;
using System.Text.Json;
using MonsterTradingCardsGameLibrary.Models;

namespace MonsterTradingCardsGame.HTTP.Handlers
{
    public class ScoreboardHandler : Handler
    {
        public override bool Handle(HttpServerEventArgs e)
        {
            if (e.Path == "/scoreboard" && e.Method == "GET")
            {
                Console.WriteLine($"[{DateTime.Now}] Received 'scoreboard' request...");

                try
                {
                    var token = e.Headers.FirstOrDefault(h => h.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                        ?.Value.Split(" ")[1];

                    if (string.IsNullOrEmpty(token) || !DatabaseHelper.IsValidToken(token))
                    {
                        e.Reply(HttpStatusCode.UNAUTHORIZED, "Invalid or missing token");
                        return true;
                    }

                    var scoreboard = DatabaseHelper.GetScoreboard();

                    if (scoreboard != null && scoreboard.Count > 0)
                    {
                        var top5 = scoreboard
                            .OrderByDescending(s => s.Elo)
                            .Take(5)
                            .ToList();

                        e.Reply(HttpStatusCode.OK, JsonSerializer.Serialize(top5));
                        Console.WriteLine($"[{DateTime.Now}] Returned Top 5 scoreboard based on Elo");
                    }
                    else
                    {
                        e.Reply(HttpStatusCode.OK, "No players found in the scoreboard");
                        Console.WriteLine($"[{DateTime.Now}] Scoreboard is empty");
                    }
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
}
