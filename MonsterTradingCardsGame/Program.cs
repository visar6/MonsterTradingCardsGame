using Microsoft.Extensions.Configuration;
using MonsterTradingCardsGame;
using MonsterTradingCardsGame.Helpers;
using MonsterTradingCardsGame.HTTP;
using Npgsql;

Console.Title = "MTCG Server";

HttpServer server = new();
server.Incoming += Svr_Incoming;

Console.WriteLine("MonsterTradindCardsGame is starting ...\n");

server.Run();

static void Svr_Incoming(object sender, HttpServerEventArgs e)
{
    Handler.HandleEvent(e);
}