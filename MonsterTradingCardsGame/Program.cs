using Microsoft.Extensions.Configuration;
using MonsterTradingCardsGame;
using MonsterTradingCardsGame.Helpers;
using MonsterTradingCardsGame.HTTP;
using Npgsql;

Console.Title = "MTCG Server";

HttpServer server = new();
server.Incoming += Server_Incoming;

Console.WriteLine("MonsterTradindCardsGame started ...\n");

server.Run();

static void Server_Incoming(object sender, HttpServerEventArgs e)
{
    Handler.HandleEvent(e);
}