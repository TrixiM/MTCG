using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

using Npgsql;

namespace MTCG.Server.Responses {

    internal class AdminResponse : HttpResponse {

        public AdminResponse(HttpRequest? IncomingRequest) : base(IncomingRequest) {

        }

        public override async Task Delete() {

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

                await DB_Functions.ResetDatabaseTables();
                Content = "OK";
                ResponseCode = 200;
                return;
            }

            catch (PostgresException) {

                Content = "Err";
                ResponseCode = 409;
            }

            catch (Exception ex) {

                Content = $"Error: {ex.ToString()}";
                ResponseCode = 400;
            }
            return;
        }

        public override async Task Get() {

            Content = "Not Implemented";
            ResponseCode = 404;
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
