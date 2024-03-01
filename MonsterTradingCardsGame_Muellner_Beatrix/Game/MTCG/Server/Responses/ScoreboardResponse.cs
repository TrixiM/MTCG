using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

using MTCG.Cards;

namespace MTCG.Server.Responses {

    internal class ScoreboardResponse : HttpResponse {

        public ScoreboardResponse(HttpRequest? IncomingRequest) : base(IncomingRequest) {

        }

        public override async Task Delete() {

            Content = "Not Implemented";
            ResponseCode = 404;
            return;
        }

        public override async Task Get() {

            try {

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

                var users = await DB_Functions.RetrieveScoreboard();

                var list = users.Select(user => new {

                    OwnerAccount = user.OwnerAccount,
                    RatingPoints = user.RatingPoints,
                    VictoryCount = user.VictoryCount,
                    Losses = user.LossCount
                }).ToList();

                Content = JsonConvert.SerializeObject(list);
                ResponseCode = 200;
                return;
            }

            catch (Exception ex) {

                Content = $"Error: {ex.ToString()}";
                ResponseCode = 400;
            }
            return;
        }

        public override async Task Post() {

            Content = "Not Implemented";
            ResponseCode = 404;
            return;
        }

        public override async Task Put() {

            Content = "Not Implemented";
            ResponseCode = 404;
            return;

        }
    }
}