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
    //Console.WriteLine(e.Method);
    //Console.WriteLine(e.Path);
    //Console.WriteLine();
    //foreach (HttpHeader i in e.Headers)
    //{
    //    Console.WriteLine(i.Name + ": " + i.Value);
    //}
    //Console.WriteLine();
    //Console.WriteLine(e.Payload);

    Handler.HandleEvent(e);
}