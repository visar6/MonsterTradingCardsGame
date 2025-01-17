using MonsterTradingCardsGame.HTTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.Helpers
{
    public static class HandlerHelper
    {
        public static bool ValidateAuthorization(HttpServerEventArgs e)
        {
            var token = e.Headers
                .FirstOrDefault(h => h.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                ?.Value.Split(" ")[1];

            if (string.IsNullOrEmpty(token) || !DatabaseHelper.IsValidToken(token))
            {
                e.Reply(HttpStatusCode.UNAUTHORIZED, "Invalid or missing token");
                return false;             }

            return true;
        }

        public static void PrintSuccess(string message)
        {
            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine(message);
            Console.ForegroundColor = previousColor;
        }

        public static void PrintWarning(string message)
        {
            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine(message);
            Console.ForegroundColor = previousColor;
        }

        public static void PrintError(string message)
        {
            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine(message);
            Console.ForegroundColor = previousColor;
        }
    }
}
