using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuth2AuthenticationBroker.Models
{
    public class OAuthLoginResult
    {
        public bool Success { get; set; }
        public OAuthResult Identity { get; set; }
        public OAuthResponseError Error { get; set; }
        
    }
}
