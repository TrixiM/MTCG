using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using MTCG.Cards;
using Newtonsoft.Json;
using Npgsql;

namespace MTCG.Server.Responses {

    internal class PackagesResponse : HttpResponse {

        public PackagesResponse(HttpRequest? IncomingRequest) : base(IncomingRequest) {

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

        public async override Task Post() {

            try {

                var token = IncomingRequest.HeaderFields["Authorization"];

                if (token == null) {

                    Content = "Access token is missing or invalid";
                    ResponseCode = 401;
                    return;
                }

                if (token != "Bearer admin-mtcgToken") {

                    Content = "Provided user is not admin";
                    ResponseCode = 403;
                    return;
                }

                var newCards = JsonConvert.DeserializeObject<List<CardData>>(IncomingRequest.Content);
                
                if (await DB_Functions.InsertNewCards(newCards)) {

                    var newPackage = new CardPackageData();

                    foreach (var card in newCards) {

                        newPackage.CardList.Add(card.PackageId);
                    }

                    if (await DB_Functions.GenerateNewPackage(newPackage)) {

                        Content = "Package and CardList successfully created";
                        ResponseCode = 201;
                        return;
                    }
                }
            }

            catch (PostgresException) {

                Content = "At least one card in the packaged already exists";
                ResponseCode = 409;
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