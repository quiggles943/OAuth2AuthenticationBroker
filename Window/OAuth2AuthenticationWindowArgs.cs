using OAuth2AuthenticationBroker.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuth2AuthenticationBroker.Window
{
    internal class OAuth2AuthenticationWindowArgs
    {
        public Dictionary<string, string> Parameters { get; set; }
        public string AuthorizeEndpoint { get; set; }

    }
}
