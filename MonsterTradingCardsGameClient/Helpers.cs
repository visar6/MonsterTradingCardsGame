using MonsterTradingCardsGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MonsterTradingCardsGameClient
{
    public static class Helpers
    {
        const string ascii = "\r\n   *                        \r\n (  `    *   )  (   (       \r\n )\\))( ` )  /(  )\\  )\\ )    \r\n((_)()\\ ( )(_)|((_)(()/(    \r\n(_()((_|_(_()))\\___ /(_))_  \r\n|  \\/  |_   _((/ __(_)) __| \r\n| |\\/| | | |  | (__  | (_ | \r\n|_|  |_| |_|   \\___|  \\___| \r\n                            \r\n";

        public static void Greet()
        {
            Console.WriteLine(ascii);

            Console.WriteLine("Welcome to the Monster Card Game!\n");
            Console.WriteLine("Select an option to continue: \n");
            Console.WriteLine("[1] Login");
            Console.WriteLine("[2] Register");
            Console.WriteLine("[3] Close\n");

            Console.Write("Please enter your choice (1, 2, or 3): ");
            
            string input = Console.ReadLine();

            while (input != "1" || input != "2" || input != "3")
            {
                if (input == "1")
                {
                    Login();
                    break;
                }
                else if (input == "2")
                {
                    Register();
                    break;
                }
                else if (input == "3")
                {
                    Environment.Exit(0);
                    break;
                }
                else
                {
                    Console.Write("Invalid input, please try again: ");
                }

                input = Console.ReadLine();
            }
        }

        public static void Login()
        {
            Console.Clear();
            Console.WriteLine(ascii);

            Console.WriteLine("Please enter your credentials ... \n");

            Console.Write("Username: ");
            string username = Console.ReadLine();

            Console.Write("Password: ");
            string password = ReadPassword();

            var result = SendLoginRequest(username, password).Result;

            if (result == "SUCCESS")
            {
                Console.Clear();
                Console.WriteLine("Login erfolgreich");
            }
            else if (result == "ERROR")
            {
                Console.Clear();
                Console.WriteLine("Login nicht erfolgreich");
            }
        }

        public static void Register()
        {

            Console.Clear();
            Console.WriteLine(ascii);
            Console.WriteLine("Please enter your credentials ... \n");

            Console.Write("Username: ");
            string username = Console.ReadLine();

            Console.Write("Password: ");
            string password = ReadPassword();

            var result = SendRegistrationRequest(username, password).Result;

            if (result == "SUCCESS")
            {
                Console.Clear();
                Greet();
            }
            else if (result == "ERROR")
            {
                Console.Clear();
                Greet();
            }
        }

        public static async Task<string> SendRegistrationRequest(string username, string password)
        {
            using (HttpClient client = new HttpClient())
            {
                var url = "http://127.0.0.1:10001/users";

                var jsonContent = new
                {
                    Username = username,
                    Password = password
                };
                
                var jsonString = JsonSerializer.Serialize(jsonContent);

                var requestData = new StringContent(jsonString, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(url, requestData);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("\nYou were registered successfully, you will be redirected shortly to the main page ... ");
                    return "SUCCESS";
                }
                else
                {
                    Console.WriteLine($"\nError while registering with status code {response.StatusCode} ...");
                    Console.WriteLine("You will be redirected to the main page ...");
                    await Task.Delay(5000);
                    return "ERROR";
                }
            }
        }

        public static async Task<string> SendLoginRequest(string username, string password)
        {
            using (HttpClient client = new HttpClient())
            {
                var url = "http://127.0.0.1:10001/sessions";

                var jsonContent = new
                {
                    Username = username,
                    Password = password
                };

                var jsonString = JsonSerializer.Serialize(jsonContent);

                var requestData = new StringContent(jsonString, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(url, requestData);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("\nYou were logged in successfully, you will be redirected shortly to your profile... ");
                    await Task.Delay(5000);
                    return "SUCCESS";
                }
                else
                {
                    Console.WriteLine($"\nError while registering with status code {response.StatusCode} ...");
                    Console.WriteLine("You will be redirected to the main page ...");
                    await Task.Delay(5000);
                    return "ERROR";
                }
            }
        }

        public static string ReadPassword()
        {
            string password = string.Empty;
            ConsoleKeyInfo key;

            while ((key = Console.ReadKey(true)).Key != ConsoleKey.Enter)
            {
                if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Substring(0, password.Length - 1);
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    password += key.KeyChar;
                }
            }

            Console.WriteLine();
            return password;
        }
    }
}