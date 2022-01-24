using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuth2AuthenticationBroker.Models
{
    public class OAuthResponseError
    {
        public string error { get; set; }
        public string error_description { get; set; }
        public string error_uri { get; set; }


        public override string ToString()
        {
            return error;
        }

    }
}
