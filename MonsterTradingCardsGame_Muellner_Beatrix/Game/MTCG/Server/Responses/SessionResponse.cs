using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using MTCG.Cards;
using MTCG.Database;
using MTCG.Server;
using Npgsql;

namespace MTCG.Server.Responses {

    internal class SessionsResponse : HttpResponse {

        public SessionsResponse(HttpRequest? IncomingRequest) : base(IncomingRequest) {

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

                var user = JsonSerializer.Deserialize<UserData>(IncomingRequest.Content);
                var CardName = user.OwnerAccount;
                var SecurePassword = user.SecurePassword;

                if (CardName == "" || SecurePassword == "") {

                    Content = "Invalid OwnerAccount/SecurePassword provided";
                    ResponseCode = 401;
                    return;
                }

                user = await DB_Functions.FetchUserDetails(CardName);

                if (!BCrypt.Net.BCrypt.Verify(SecurePassword, user.SecurePassword)) {

                    Content = "Invalid OwnerAccount/SecurePassword provided";
                    ResponseCode = 401;
                    return;
                }

                var token = $"Bearer {user.OwnerAccount}-mtcgToken";
                await DB_Functions.AuthenticateUser(token, user);

                Content = token;
                ResponseCode = 200;
            }

            catch (PostgresException) {

                Content = "Invalid OwnerAccount/SecurePassword provided";
                ResponseCode = 401;
            }

            catch (Exception ex) {

                Content = $"Error: {ex.ToString}";
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