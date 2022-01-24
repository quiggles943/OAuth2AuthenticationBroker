using OAuth2AuthenticationBroker.Models;
using OAuth2AuthenticationBroker.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuth2AuthenticationBroker.Exceptions
{
    internal class OAuth2AuthenticationException : Exception
    {
        internal OAuthResponseError Error;
        public OAuth2AuthenticationException() { }
        public OAuth2AuthenticationException(string message) : base(message) { }
        public OAuth2AuthenticationException(string message, Exception inner) : base(message, inner) { }
        public OAuth2AuthenticationException(string message, OAuthResponseError error) { Error = error; }
        protected OAuth2AuthenticationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
