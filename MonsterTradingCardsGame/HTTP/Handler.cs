using Npgsql;
using System.Reflection;
using System.Text.Json;
using MonsterTradingCardsGame.Helpers;
using MonsterTradingCardsGameLibrary.Models;

namespace MonsterTradingCardsGame.HTTP
{
    public abstract class Handler : IHandler
    {
        private static List<IHandler>? handlers = null;

        private static List<IHandler> GetHandlers()
        {
            List<IHandler> handlers = new();

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes()
                              .Where(t => t.IsAssignableTo(typeof(IHandler)) && (!t.IsAbstract)))
            {
                IHandler? handler = (IHandler?)Activator.CreateInstance(type);

                if (handlers != null)
                {
                    handlers.Add(handler);
                }
            }

            return handlers;
        }

        public static void HandleEvent(HttpServerEventArgs e)
        {
            handlers ??= GetHandlers();

            foreach (IHandler handler in handlers)
            {
                if (handler.Handle(e)) return;
            }

            e.Reply(HttpStatusCode.NOT_FOUND, "Route not found");
        }

        public abstract bool Handle(HttpServerEventArgs e);
    }       
}