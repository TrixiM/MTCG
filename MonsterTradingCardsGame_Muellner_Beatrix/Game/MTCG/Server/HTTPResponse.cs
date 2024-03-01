using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

using MTCG.Database;
using MTCG.Server;

namespace MTCG.Server {

    public abstract class HttpResponse {

        public DB_Functions DB_Functions { get; set; }
        public HttpRequest IncomingRequest { get; set; }
        public int ResponseCode { get; set; }
        public string Content { get; set; }

        public HttpResponse(HttpRequest request) {

            DB_Functions = new DB_Functions();
            IncomingRequest = request;
        }

        public void Transmit(StreamWriter responseWriter) {

            try {

                SendResponseToClient(responseWriter);
            }

            catch (Exception ex) {

                Console.WriteLine($"Ein Fehler ist aufgetreten: {ex.Message}");
            }
        }

        public void SendResponseToClient(StreamWriter responseWriter) {

            InitializeResponseHeader(responseWriter);

            if (Content != null) {

                SendResponseBody(responseWriter);
            }
            FinalizeResponse(responseWriter);
        }

        public void InitializeResponseHeader(StreamWriter writer) {

            WriteResponseLine(writer, $"HTTP/1.1 {ResponseCode}");
            WriteResponseLine(writer, $"Current Time: {DateTime.Now}");
            WriteResponseLine(writer, "Server: MonsterCardTradingGame Server");
        }

        public void SendResponseBody(StreamWriter writer) {

            WriteResponseLine(writer, $"Content-Length: {Content.Length}");
            WriteResponseLine(writer, "Content-CardType: text/html; charset=utf-8");
            WriteResponseLine(writer, "");
            WriteResponseLine(writer, Content);
        }

        public void FinalizeResponse(StreamWriter writer) {

            WriteResponseLine(writer, "");
        }

        public void WriteResponseLine(StreamWriter writer, string line) {

            Console.WriteLine(line);
            writer.WriteLine(line);
            writer.Flush();
        }

        public abstract Task Put();

        public abstract Task Get();

        public abstract Task Post();

        public abstract Task Delete();
    }
}
