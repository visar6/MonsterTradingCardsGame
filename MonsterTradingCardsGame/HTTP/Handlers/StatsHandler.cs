﻿using MonsterTradingCardsGame.Helpers;
using System.Text.Json;

namespace MonsterTradingCardsGame.HTTP.Handlers
{
    public class StatsHandler : Handler
    {
        private readonly IDatabaseHelper databaseHelper;

        public StatsHandler(IDatabaseHelper databaseHelper)
        {
            this.databaseHelper = databaseHelper;
        }

        public override bool Handle(HttpServerEventArgs e)
        {
            if (e.Path == "/stats" && e.Method == "GET")
            {
                Console.WriteLine($"[{DateTime.Now}] Received 'stats' request...");

                try
                {
                    var token = e.Headers.FirstOrDefault(h => h.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                        ?.Value.Split(" ")[1];

                    if (string.IsNullOrEmpty(token) || !databaseHelper.IsValidToken(token))
                    {
                        e.Reply(HttpStatusCode.UNAUTHORIZED);
                        HandlerHelper.PrintError($"[{DateTime.Now}] Invalid or missing token");
                        return true;
                    }

                    var username = databaseHelper.GetUsernameFromToken(token);
                    if (string.IsNullOrEmpty(username))
                    {
                        e.Reply(HttpStatusCode.UNAUTHORIZED);
                        HandlerHelper.PrintError($"[{DateTime.Now}] User not found for token: {token}");
                        return true;
                    }

                    var user = databaseHelper.GetUser(username);
                    var stats = databaseHelper.GetUserStats(user.Id);

                    if (stats != null)
                    {
                        if (stats.Elo >= 0 || stats.Wins >= 0 || stats.Losses >= 0)
                        {
                            e.Reply(HttpStatusCode.OK, JsonSerializer.Serialize(stats));
                            HandlerHelper.PrintSuccess($"[{DateTime.Now}] Returned stats for user '{username}': Elo={stats.Elo}, Wins={stats.Wins}, Losses={stats.Losses}");
                        }
                        else
                        {
                            e.Reply(HttpStatusCode.OK);
                            HandlerHelper.PrintWarning($"[{DateTime.Now}] User '{username}' has no statistics (all values are 0)");
                        }
                    }
                    else
                    {
                        e.Reply(HttpStatusCode.NOT_FOUND);
                        HandlerHelper.PrintError($"[{DateTime.Now}] Stats not found for user '{username}'");
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
