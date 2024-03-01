using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

using MTCG.Cards;

namespace MTCG.Server.Responses {

    internal class TransactionsResponse : HttpResponse {

        public TransactionsResponse(HttpRequest? IncomingRequest) : base(IncomingRequest) {

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

                if (tokenUser.Coins < 5) {

                    Content = "Not enough money for buying a card package";
                    ResponseCode = 403;
                    return;
                }

                if (await DB_Functions.PurchaseCardPackage(tokenUser)) {

                    Content = "Packages bought";
                    ResponseCode = 200;
                    return;
                }

                else {

                    Content = "No card package available for buying";
                    ResponseCode = 404;
                    return;
                }
            }

            catch (Exception ex) {

                Content = $"Error: {ex.ToString()}";
                ResponseCode = 400;
            }
            return;
        }

        public override async Task Put() {

            Content = "Not Implemented";
            ResponseCode = 404;
            return;
        }
    }
}