using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.HTTP
{
    public sealed class HttpServer
    {
        private TcpListener? listener;

        public event HttpServerEventHandler? Incoming;

        public bool Active
        {
            get; private set;
        } = false;

        public void Run()
        {
            if (Active) return;

            Active = true;
            listener = new(IPAddress.Parse("127.0.0.1"), 10001);
            listener.Start();

            byte[] buf = new byte[256];

            while (Active)
            {
                TcpClient client = listener.AcceptTcpClient();
                string data = string.Empty;

                while (client.GetStream().DataAvailable || string.IsNullOrWhiteSpace(data))
                {
                    int n = client.GetStream().Read(buf, 0, buf.Length);
                    data += Encoding.ASCII.GetString(buf, 0, n);
                }

                Incoming?.Invoke(this, new(client, data));
            }
        }

        public void Stop()
        {
            Active = false;
            listener?.Stop();
        }
    }
}