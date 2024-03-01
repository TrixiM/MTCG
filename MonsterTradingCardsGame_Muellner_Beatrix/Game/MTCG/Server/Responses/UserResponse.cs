using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using MTCG.Cards;
using MTCG.Database;
using Npgsql;

namespace MTCG.Server.Responses {

    public class UsersResponse : HttpResponse {

        public UsersResponse(HttpRequest? IncomingRequest) : base(IncomingRequest) {

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

                if (token != "Bearer admin-mtcgToken") {

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
                }

                var OwnerAccount = IncomingRequest.EndpointParameter;
                var user = await DB_Functions.FetchUserDetails(OwnerAccount);

                if (user != null) {

                    if (user.OwnerAccount == tokenUser.OwnerAccount || token == "Bearer admin-mtcgToken") {

                        Content = JsonSerializer.Serialize(user);
                        ResponseCode = 200;
                        return;
                    }

                    Content = "Access token is missing or invalid";
                    ResponseCode = 401;
                }

                else {

                    Content = "User not found";
                    ResponseCode = 404;
                }
            }

            catch (Exception ex) {

                Content = $"Error: {ex.ToString()}";
                ResponseCode = 400;
            }
            return;
        }

        public override async Task Post() {

            try {

                var user = JsonSerializer.Deserialize<UserData>(IncomingRequest.Content);

                if (await DB_Functions.RegisterUser(user)) {

                    Content = "User successfully created";
                    ResponseCode = 201;
                }

                else {

                    Content = "User with same OwnerAccount already registered";
                    ResponseCode = 409;
                }
            }

            catch (PostgresException) {

                Content = "User with same OwnerAccount already registered";
                ResponseCode = 409;
            }

            catch (Exception ex) {

                Content = $"Error: {ex.ToString()}";
                ResponseCode = 400;
            }
            return;
        }

        public override async Task Put() {

            try {

                UserData tokenUser = new UserData();
                var token = IncomingRequest.HeaderFields["Authorization"];
                
                if (token != "Bearer admin-mtcgToken") {

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
                }

                var OwnerAccount = IncomingRequest.EndpointParameter;
                var user = await DB_Functions.FetchUserDetails(OwnerAccount);
            }

            catch (Exception ex) {
                
                Content = $"Error: {ex.ToString()}";
                ResponseCode = 400;

            }
            return;
        }
    }
}