using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Server.Responses {

    internal class DefaultResponse : HttpResponse {

        public DefaultResponse(HttpRequest? IncomingRequest) : base(IncomingRequest) {

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
