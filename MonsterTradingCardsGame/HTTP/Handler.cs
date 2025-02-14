﻿using Npgsql;
using System.Reflection;
using System.Text.Json;
using MonsterTradingCardsGame.Helpers;
using MonsterTradingCardsGameLibrary.Models;

namespace MonsterTradingCardsGame.HTTP
{
    public abstract class Handler : IHandler
    {
        private static List<IHandler>? handlers = null;
        private static List<IHandler> GetHandlers(IDatabaseHelper databaseHelper)
        {
            List<IHandler> handlers = new();

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes()
                              .Where(t => typeof(IHandler).IsAssignableFrom(t) && !t.IsAbstract))
            {
                ConstructorInfo? ctor = type.GetConstructors()
                    .FirstOrDefault(c => c.GetParameters().Length == 1 && c.GetParameters()[0].ParameterType == typeof(IDatabaseHelper));

                if (ctor != null)
                {
                    IHandler? handler = (IHandler?)ctor.Invoke(new object[] { databaseHelper });
                    if (handler != null)
                    {
                        handlers.Add(handler);
                    }
                }
                else
                {
                    Console.WriteLine($"Skipping {type.Name} - No suitable constructor found.");
                }
            }

            return handlers;
        }


        public static void HandleEvent(HttpServerEventArgs e)
        {
            handlers ??= GetHandlers(new DatabaseHelper());

            ThreadPool.QueueUserWorkItem(_ =>
            {
                foreach (IHandler handler in handlers)
                {
                    if (handler.Handle(e)) return;
                }

                e.Reply(HttpStatusCode.NOT_FOUND, "Route not found");
            });
        }

        public abstract bool Handle(HttpServerEventArgs e);
    }       
}