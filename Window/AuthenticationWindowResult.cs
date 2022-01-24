using OAuth2AuthenticationBroker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuth2AuthenticationBroker.Window
{
    public class AuthenticationWindowResult
    {
        public bool Success { get; set; }
        public string AuthCode { get; set; }
        public OAuthResponseError Error { get; set; }
    }
}
