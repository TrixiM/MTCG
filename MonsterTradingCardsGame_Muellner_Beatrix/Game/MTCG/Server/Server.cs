using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using MTCG.Server.Responses;
using MTCG.Database;

namespace MTCG.Server {

    public class Server {

        public static TcpListener networkListener;

        public static void StartServer(int port) {

            networkListener = new TcpListener(IPAddress.Loopback, port);
            networkListener.Start();
            Console.WriteLine("Server is waiting for requests...");

            while (true) {

                if (networkListener.Pending()) {

                    TcpClient clientSocket = networkListener.AcceptTcpClient();
                    Server requestHandler = new Server(clientSocket);
                    Task.Run(() => requestHandler.ProcessRequestAsync());
                }
            }
        }

        public TcpClient clientConnection;
        public HttpRequest httpRequest = new HttpRequest();
        public HttpResponse httpResponse;

        public Server(TcpClient socket) {

            this.clientConnection = socket;
        }

        public async Task ProcessRequestAsync() {

            using (var networkStream = clientConnection.GetStream())
            using (var reader = new StreamReader(networkStream))
            using (var writer = new StreamWriter(networkStream) { AutoFlush = true }) {

                await ReadRequestAsync(reader);
                ConstructResponse();
                await ExecuteResponseAsync();
                await SendResponseAsync(writer);
            }
        }

        public async Task ReadRequestAsync(StreamReader reader) {

            string line;

            while ((line = await reader.ReadLineAsync()) != null) {

                if (line.Length == 0) break;

                ParseRequestLine(line);
                ParseHeaders(line);
            }
            await ParseRequestBodyAsync(reader);
        }

        public void ParseRequestLine(string line) {

            if (httpRequest.RequestMethod == null) {

                string[] requestParts = line.Split(' ');
                if (requestParts.Length >= 3) {

                    httpRequest.RequestMethod = requestParts[0];
                    httpRequest.Endpoint = requestParts[1];
                    httpRequest.ProtocolVersion = requestParts[2];
                }
            }
        }

        public void ParseHeaders(string line) {

            if (line.Contains(": ")) {

                string[] parts = line.Split(new[] { ": " }, 2, StringSplitOptions.None);
                httpRequest.HeaderFields[parts[0]] = parts[1];
            }
        }

        public async Task ParseRequestBodyAsync(StreamReader reader) {

            if (httpRequest.HeaderFields.TryGetValue("Content-Length", out string contentLengthValue) && int.TryParse(contentLengthValue, out int contentLength)) {

                char[] buffer = new char[contentLength];
                await reader.ReadAsync(buffer, 0, contentLength);
                httpRequest.Content = new string(buffer);
            }
        }

        public void ConstructResponse() {

            httpResponse = httpRequest.Endpoint switch {

                "users" => new UsersResponse(httpRequest),
                "sessions" => new SessionsResponse(httpRequest),
                "packages" => new PackagesResponse(httpRequest),
                "transactions" => new TransactionsResponse(httpRequest),
                "CardList" => new CardsResponse(httpRequest),
                "deck" => new DeckResponse(httpRequest),
                "stats" => new StatsResponse(httpRequest),
                "scoreboard" => new ScoreboardResponse(httpRequest),
                "battles" => new BattlesResponse(httpRequest),
                "admin" => new AdminResponse(httpRequest),
                _ => new DefaultResponse(httpRequest),
            };
        }

        public async Task ExecuteResponseAsync() {

            if (httpRequest.RequestMethod == "GET") {

                await httpResponse.Get();
            }

            else if (httpRequest.RequestMethod == "POST") {

                await httpResponse.Post();
            }

            else if (httpRequest.RequestMethod == "PUT") {

                await httpResponse.Put();
            }

            else if (httpRequest.RequestMethod == "DELETE") {

                await httpResponse.Delete();
            }
        }

        public async Task SendResponseAsync(StreamWriter writer) {

            await Task.Run(() => httpResponse.Transmit(writer));
        }
    }
}
