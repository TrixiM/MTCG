using System;
using System.Linq;


namespace MTCG.Server {

    public class HttpRequest {

        public string Content { get; set; }
        public string RequestMethod { get; set; }
        public string ProtocolVersion { get; set; }
        public string Endpoint { get; set; }
        public string EndpointParameter { get; set; }

        public Dictionary<string, string> HeaderFields { get; set; } = new Dictionary<string, string>();

        public void AddHeaderPair(string key, string value) {

            HeaderFields.Add(key, value);
        }
    }
}
