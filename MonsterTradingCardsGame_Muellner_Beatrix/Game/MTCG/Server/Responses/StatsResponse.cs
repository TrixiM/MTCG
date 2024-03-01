using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Reflection.Metadata;

using MTCG.Cards;


namespace MTCG.Server.Responses {

    internal class StatsResponse : HttpResponse {

        public StatsResponse(HttpRequest? IncomingRequest) : base(IncomingRequest) {

        }

        public double CalculateWinLossRatio(UserData user) {

            if (user.LossCount == 0) {

                return user.VictoryCount > 0 ? double.PositiveInfinity : 0;
            }

            return (double)user.VictoryCount / user.LossCount;
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

                double winLoseRatio = CalculateWinLossRatio(tokenUser);

                var content = new {

                    CardName = tokenUser.OwnerAccount,
                    RatingPoints = tokenUser.RatingPoints,
                    VictoryCount = tokenUser.VictoryCount,
                    Losses = tokenUser.LossCount,
                    WinLoseRatio = winLoseRatio
                };

                Content = JsonSerializer.Serialize(content);
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