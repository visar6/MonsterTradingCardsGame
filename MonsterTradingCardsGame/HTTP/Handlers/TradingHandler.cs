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
                    e.Reply(HttpStatusCode.BAD_REQUEST, "Invalid trading deal format");
                    return;
                }

                if (string.IsNullOrEmpty(tradingDeal.Id))
                {
                    tradingDeal.Id = Guid.NewGuid().ToString();
                }

                tradingDeal.UserId = user.Id;

                if (!DatabaseHelper.UserOwnsCard(user.Id, tradingDeal.CardId))
                {
                    e.Reply(HttpStatusCode.FORBIDDEN, "You do not own this card");
                    return;
                }

                if (DatabaseHelper.CreateTradingDeal(tradingDeal))
                {
                    e.Reply(HttpStatusCode.CREATED, "Trading deal created successfully");
                }
                else
                {
                    e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR, "Failed to create trading deal");
                }
            }
            catch (JsonException)
            {
                e.Reply(HttpStatusCode.BAD_REQUEST, "Invalid JSON format");
            }
        }

        private void GetTradingDeals(HttpServerEventArgs e)
        {
            List<TradingDeal> deals = DatabaseHelper.GetAllTradingDeals();
            e.Reply(HttpStatusCode.OK, JsonSerializer.Serialize(deals));
        }

        private void DeleteTradingDeal(HttpServerEventArgs e)
        {
            User? user = GetUserFromRequest(e);
            if (user == null) return;

            string tradingDealId = e.Path.Split('/').Last();

            if (!DatabaseHelper.TradingDealExists(tradingDealId))
            {
                e.Reply(HttpStatusCode.NOT_FOUND, "Trading deal not found");
                return;
            }

            if (!DatabaseHelper.UserOwnsTradingDeal(user.Id, tradingDealId))
            {
                e.Reply(HttpStatusCode.FORBIDDEN, "You are not the owner of this trading deal");
                return;
            }

            if (DatabaseHelper.DeleteTradingDeal(tradingDealId, user.Id))
            {
                e.Reply(HttpStatusCode.OK, "Trading deal deleted successfully");
            }
            else
            {
                e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR, "Failed to delete trading deal");
            }
        }

        private void ExecuteTrade(HttpServerEventArgs e)
        {
            User? user = GetUserFromRequest(e);
            if (user == null) return;

            string tradingDealId = e.Path.Split('/').Last();

            if (!DatabaseHelper.TradingDealExists(tradingDealId))
            {
                e.Reply(HttpStatusCode.NOT_FOUND, "Trading deal not found");
                return;
            }

            var offeredCardId = JsonSerializer.Deserialize<string>(e.Payload);
            if (string.IsNullOrEmpty(offeredCardId) || !DatabaseHelper.UserOwnsCard(user.Id, offeredCardId))
            {
                e.Reply(HttpStatusCode.BAD_REQUEST, "Invalid offered card");
                return;
            }

            TradingDeal? deal = DatabaseHelper.GetTradingDeal(tradingDealId);
            if (deal == null)
            {
                e.Reply(HttpStatusCode.NOT_FOUND, "Trading deal no longer exists");
                return;
            }

            if (deal.UserId == user.Id)
            {
                e.Reply(HttpStatusCode.FORBIDDEN, "You cannot trade with yourself");
                return;
            }

            if (!DatabaseHelper.CardMeetsTradingRequirements(offeredCardId, deal))
            {
                e.Reply(HttpStatusCode.BAD_REQUEST, "Your card does not meet the trade requirements");
                return;
            }

            if (DatabaseHelper.ExecuteTrade(user.Id, offeredCardId, deal))
            {
                e.Reply(HttpStatusCode.OK, "Trade successful");
            }
            else
            {
                e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR, "Trade execution failed");
            }
        }

        private User? GetUserFromRequest(HttpServerEventArgs e)
        {
            var token = e.Headers.FirstOrDefault(h => h.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase))?.Value.Split(" ")[1];
            if (string.IsNullOrEmpty(token) || !DatabaseHelper.IsValidToken(token))
            {
                e.Reply(HttpStatusCode.UNAUTHORIZED, "Invalid or missing token");
                return null;
            }

            string? username = DatabaseHelper.GetUsernameFromToken(token);
            if (string.IsNullOrEmpty(username))
            {
                e.Reply(HttpStatusCode.UNAUTHORIZED, "Unauthorized: Invalid token");
                return null;
            }

            return DatabaseHelper.GetUser(username);
        }
    }
}
