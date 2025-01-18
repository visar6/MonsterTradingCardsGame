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
        public static string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return char.ToUpper(input[0]) + input.Substring(1).ToLower();
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
