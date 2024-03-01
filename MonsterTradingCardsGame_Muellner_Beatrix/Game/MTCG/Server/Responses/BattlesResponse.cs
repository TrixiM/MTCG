using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Metadata;
using System.Runtime.Intrinsics.X86;

using MTCG.Database;
using MTCG.BattleLogic;
using MTCG.Cards;


namespace MTCG.Server.Responses {

    internal class BattlesResponse : HttpResponse {

        public static ConcurrentQueue<string> PlayerLineup = new ConcurrentQueue<string>();

        public BattlesResponse(HttpRequest? IncomingRequest) : base(IncomingRequest) {

        }

        public override async Task Delete() {

            Content = "Not Implemented";
            ResponseCode = 404;
            return;
        }

        public override async Task Get() {

            Content = "Not Implemented";
            ResponseCode = 404;
            return;
        }

        public override async Task Post() {

            UserData tokenUser = new UserData();
            var token = IncomingRequest.HeaderFields["Authorization"];

            if (token == "") {

                Content = "Access token is missing or invalid";
                ResponseCode = 401;
                return;
            }

            tokenUser = await DB_Functions.RetrieveUserBySession(token);

            if (tokenUser == null) {

                Content = "Access token is missing or invalid";
                ResponseCode = 401;
                return;
            }

            PlayerLineup.Enqueue(token);

            if (PlayerLineup.Count >= 2) {

                if (PlayerLineup.TryDequeue(out string player1Token) && PlayerLineup.TryDequeue(out string player2Token)) {

                    await StartBattle(player1Token, player2Token);
                }
            }

            else {

                Content = "Waiting for another player to join";
                ResponseCode = 202;
            }
            return;
        }

        public async Task StartBattle(string player1Token, string player2Token) {

            var playerOne = await DB_Functions.RetrieveUserBySession(player1Token);
            var playerTwo = await DB_Functions.RetrieveUserBySession(player2Token);

            Battle_Functions battle = new Battle_Functions(playerOne, playerTwo, await DB_Functions.RetrieveUserDeck(playerOne), await DB_Functions.RetrieveUserDeck(playerTwo), new DB_Functions());
            await battle.StartGameBattle();

            Content = "Battle started between two players";
            ResponseCode = 200;
        }

        public override async Task Put() {

            Content = "Not Implemented";
            ResponseCode = 404;
            return;

        }
    }
}
