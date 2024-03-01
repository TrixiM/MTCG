using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

using MTCG.Cards;
using Newtonsoft.Json;

namespace MTCG.Server.Responses {

    internal class DeckResponse : HttpResponse {

        public DeckResponse(HttpRequest? IncomingRequest) : base(IncomingRequest) {

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

                var CardList = await DB_Functions.RetrieveUserDeck(tokenUser);
                
                if (CardList.Count() == 0) {

                    Content = "The IncomingRequest was fine, but the user doesn't have any CardList";
                    ResponseCode = 204;
                    return;
                }

                Content = JsonConvert.SerializeObject(CardList);
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

            try {

                var token = IncomingRequest.HeaderFields["Authorization"];

                if (token == "") {

                    Content = "Access token is missing or invalid";
                    ResponseCode = 401;
                    return;
                }

                var CardList = JsonConvert.DeserializeObject<List<string>>(IncomingRequest.Content);

                if (CardList == null || CardList.Count < 4) {

                    Content = "The provided deck did not include the required amount of CardList";
                    ResponseCode = 400;
                    return;
                }

                if (!await DB_Functions.UpdateUserDeck(CardList, token)) {

                    Content = "At least one of the provided CardList does not belong to the user or is not available";
                    ResponseCode = 403;
                    return;
                }

                Content = "The deck has been successfully configured";
                ResponseCode = 200;
                return;
            }

            catch (Exception ex) {

                Content = $"Error: {ex.ToString()}";
                ResponseCode = 400;
            }
            return;
        }
    }
}