﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.HTTP
{
    public class HttpServerEventArgs : EventArgs
    {
        public HttpServerEventArgs()
        {
            
        }

        public HttpServerEventArgs(TcpClient client, string plainMessage)
        {
            this.client = client;

            PlainMessage = plainMessage;
            Payload = string.Empty;

            string[] lines = plainMessage.Replace("\r\n", "\n").Split('\n');
            bool inHeaders = true;
            List<HttpHeader> headers = new();

            for (int i = 0; i < lines.Length; i++)
            {
                if (i == 0)
                {
                    string[] inc = lines[0].Split(' ');
                    Method = inc[0];
                    Path = inc[1];
                    continue;
                }

                if (inHeaders)
                {
                    if (string.IsNullOrWhiteSpace(lines[i]))
                    {
                        inHeaders = false;
                    }
                    else { headers.Add(new(lines[i])); }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(Payload)) { Payload += "\r\n"; }
                    Payload += lines[i];
                }
            }

            Headers = headers.ToArray();
        }

        protected TcpClient client;

        public string PlainMessage { get; protected set; } = string.Empty;

        public virtual string Method { get;  set; } = string.Empty;

        public virtual string Path { get;  set; } = string.Empty;

        public virtual HttpHeader[] Headers { get; set; } = Array.Empty<HttpHeader>();

        public virtual string Payload { get;  set; } = string.Empty;

        public void Reply(int status, string? body = null)
        {
            string data;

            switch (status)
            {
                case 200:
                    data = "HTTP/1.1 200 OK\n"; break;
                case 201: 
                    data = "HTTP/1.1 201 Created\n"; break;
                case 400:
                    data = "HTTP/1.1 400 Bad Request\n"; break;
                case 401:
                    data = "HTTP/1.1 401 Unauthorized\n"; break;
                case 403:
                    data = "HTTP/1.1 403 Forbidden\n"; break;
                case 404:
                    data = "HTTP/1.1 404 Not found\n"; break;
                case 409:
                    data = "HTTP/1.1 409 Conflict\n"; break;
                case 500:
                    data = "HTTP/1.1 500 Internal Server Error\n"; break;
                default:
                    data = $"HTTP/1.1 {status} Status unknown\n"; break;
            }

            if (string.IsNullOrEmpty(body))
            {
                data += "Content-Length: 0\n";
            }

            data += "Content-Type: text/plain\n\n";

            if (!string.IsNullOrEmpty(body)) { data += body; }

            byte[] buffer = Encoding.ASCII.GetBytes(data);
            
            client.GetStream().Write(buffer, 0, buffer.Length);
            client.Close();
            client.Dispose();
        }
    }
}