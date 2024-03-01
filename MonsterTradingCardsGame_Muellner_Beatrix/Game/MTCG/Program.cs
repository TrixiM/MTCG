using System.Net.Sockets;

using MTCG.Database;

namespace MTCG {

    internal class Program {

        static void Main(string[] args) {

            Server.Server.StartServer(10001);
        }
    }

}