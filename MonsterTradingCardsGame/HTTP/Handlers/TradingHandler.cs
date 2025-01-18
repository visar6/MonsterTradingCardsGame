using MonsterTradingCardsGame.Helpers;
using MonsterTradingCardsGameLibrary.Models;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;
using MonsterTradingCardsGameLibrary.Models.MonsterTradingCardsGameLibrary.Models;

namespace MonsterTradingCardsGame.HTTP.Handlers
{
    public class TradingHandler : Handler
    {
        private readonly IDatabaseHelper databaseHelper;

        public TradingHandler(IDatabaseHelper databaseHelper)
        {
            this.databaseHelper = databaseHelper;
        }

        public override bool Handle(HttpServerEventArgs e)
        {
            if (e.Method == "POST" && e.Path == "/tradings")
            {
                CreateTradingDeal(e);
                return true;
            }
            else if (e.Method == "GET" && e.Path == "/tradings")
            {
                GetTradingDeals(e);
                return true;
            }
            else if (e.Method == "DELETE" && e.Path.StartsWith("/tradings/"))
            {
                DeleteTradingDeal(e);
                return true;
            }
            else if (e.Method == "POST" && e.Path.StartsWith("/tradings/"))
            {
                ExecuteTrade(e);
                return true;
            }
            return false;
        }

        private void CreateTradingDeal(HttpServerEventArgs e)
        {
            User? user = GetUserFromRequest(e);
            if (user == null) return;

            try
            {
                var tradingDeal = JsonSerializer.Deserialize<TradingDeal>(e.Payload);
                if (tradingDeal == null || string.IsNullOrEmpty(tradingDeal.CardId))
                {
                    e.Reply(HttpStatusCode.BAD_REQUEST);
                    return;
                }

                if (string.IsNullOrEmpty(tradingDeal.Id))
                {
                    tradingDeal.Id = Guid.NewGuid().ToString();
                }

                tradingDeal.UserId = user.Id;

                if (!databaseHelper.UserOwnsCard(user.Id, tradingDeal.CardId))
                {
                    e.Reply(HttpStatusCode.FORBIDDEN);
                    return;
                }

                if (databaseHelper.CreateTradingDeal(tradingDeal))
                {
                    HandlerHelper.PrintSuccess($"[{DateTime.Now}] Trading deal successfully created.");
                    e.Reply(HttpStatusCode.OK);
                }
                else
                {
                    HandlerHelper.PrintError($"[{DateTime.Now}] Error creating trading deal.");
                    e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR);
                }
            }
            catch (JsonException)
            {
                e.Reply(HttpStatusCode.BAD_REQUEST);
            }
        }

        private void GetTradingDeals(HttpServerEventArgs e)
        {
            List<TradingDeal> deals = databaseHelper.GetAllTradingDeals();
            e.Reply(HttpStatusCode.OK, JsonSerializer.Serialize(deals));
            HandlerHelper.PrintSuccess($"[{DateTime.Now}] Successfully retrieved all trading deals.");
        }

        private void DeleteTradingDeal(HttpServerEventArgs e)
        {
            User? user = GetUserFromRequest(e);
            if (user == null) return;

            string tradingDealId = e.Path.Split('/').Last();

            if (!databaseHelper.TradingDealExists(tradingDealId))
            {
                e.Reply(HttpStatusCode.NOT_FOUND);
                return;
            }

            if (!databaseHelper.UserOwnsTradingDeal(user.Id, tradingDealId))
            {
                e.Reply(HttpStatusCode.FORBIDDEN);
                return;
            }

            if (databaseHelper.DeleteTradingDeal(tradingDealId, user.Id))
            {
                HandlerHelper.PrintSuccess($"[{DateTime.Now}] Trading deal {tradingDealId} successfully deleted.");
                e.Reply(HttpStatusCode.OK);
            }
            else
            {
                HandlerHelper.PrintError($"[{DateTime.Now}] Error deleting trading deal {tradingDealId}.");
                e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR);
            }
        }

        private void ExecuteTrade(HttpServerEventArgs e)
        {
            User? user = GetUserFromRequest(e);
            if (user == null) return;

            string tradingDealId = e.Path.Split('/').Last();

            if (!databaseHelper.TradingDealExists(tradingDealId))
            {
                e.Reply(HttpStatusCode.NOT_FOUND);
                return;
            }

            var offeredCardId = JsonSerializer.Deserialize<string>(e.Payload);
            if (string.IsNullOrEmpty(offeredCardId) || !databaseHelper.UserOwnsCard(user.Id, offeredCardId))
            {
                e.Reply(HttpStatusCode.BAD_REQUEST);
                return;
            }

            TradingDeal? deal = databaseHelper.GetTradingDeal(tradingDealId);
            if (deal == null)
            {
                e.Reply(HttpStatusCode.NOT_FOUND);
                return;
            }

            if (deal.UserId == user.Id)
            {
                e.Reply(HttpStatusCode.FORBIDDEN);
                return;
            }

            if (!databaseHelper.CardMeetsTradingRequirements(offeredCardId, deal))
            {
                e.Reply(HttpStatusCode.BAD_REQUEST);
                return;
            }

            if (databaseHelper.ExecuteTrade(user.Id, offeredCardId, deal))
            {
                HandlerHelper.PrintSuccess($"[{DateTime.Now}] Trade successfully executed.");
                e.Reply(HttpStatusCode.OK);
            }
            else
            {
                HandlerHelper.PrintError($"[{DateTime.Now}] Error executing trade.");
                e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR);
            }
        }

        private User? GetUserFromRequest(HttpServerEventArgs e)
        {
            var token = e.Headers.FirstOrDefault(h => h.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase))?.Value.Split(" ")[1];
            if (string.IsNullOrEmpty(token) || !databaseHelper.IsValidToken(token))
            {
                e.Reply(HttpStatusCode.UNAUTHORIZED);
                return null;
            }

            string? username = databaseHelper.GetUsernameFromToken(token);
            if (string.IsNullOrEmpty(username))
            {
                e.Reply(HttpStatusCode.UNAUTHORIZED);
                return null;
            }

            return databaseHelper.GetUser(username);
        }
    }
}
