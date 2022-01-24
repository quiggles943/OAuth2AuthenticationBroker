using Newtonsoft.Json;
using OAuth2AuthenticationBroker.Enums;
using OAuth2AuthenticationBroker.Exceptions;
using OAuth2AuthenticationBroker.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Web;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace OAuth2AuthenticationBroker.Window
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AuthenticationWindow : Page
    {
        private string AuthorizeEndpoint { get; set; }
        private event RoutedEventHandler AuthCodeRetrieved;
        public static event RoutedEventHandler Complete;
        public AuthenticationWindow()
        {
            this.InitializeComponent();
            webView.Focus(FocusState.Pointer);
            webView.UnsupportedUriSchemeIdentified += WebView_UnsupportedUriSchemeIdentified;
            webView.NavigationFailed += WebView_NavigationFailed;
            AuthCodeRetrieved += OAuth2AuthenticationBroker_AuthCodeRetrieved;
        }

        private void WebView_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            AuthenticationWindowResult loginResult = new AuthenticationWindowResult() { Success = false, Error = new OAuthResponseError() { error = e.WebErrorStatus.ToString(), error_description = e.WebErrorStatus.ToString() } };
            Complete?.Invoke(loginResult, new RoutedEventArgs());
        }

        private async void OAuth2AuthenticationBroker_AuthCodeRetrieved(object sender, RoutedEventArgs e)
        {
            string authCode = (string)sender;
            webView.NavigateToString("<html>"+
            "<body>"+
            "<div style=\"width: 100%;margin: 0 auto;\">"+
                "<div style=\"width: 100%;min-height: 100vh;display: -webkit-box;display: -webkit-flex;display: -moz-box;display: -ms-flexbox;display: flex;flex-wrap: wrap;justify-content: center;padding: 15px;background-repeat: no-repeat;background-position: center;background-size: cover;vertical-align: middle;margin-left: auto;margin-right: auto;align-content: space-around;overflow: hidden;\">"+
                    "<div style=\"width: 500px;background: #fff;overflow: hidden;max-width: 500px;display: table-cell;vertical-align: middle;width: calc(100% - 40px);box-shadow: 0 2px 6px rgb(0 0 0 / 20%);padding: 20px;padding-bottom: 40px;min-width: 412px;\">"+
                        "<form style=\"width: 100%;\">"+
                            "<div style = \"margin:5px;\">"+
                                "<div style=\"margin-left: auto;margin-right: auto; margin-top: 10px;margin-bottom: 10px;vertical-align: middle;text-align:center;\">"+
						            "<h1 style=\"font-family: sans-serif; font-weight:normal;\">Authentication Broker</h1>"+
                                "</div>"+
                            "</div>"+
                            "<div style=\"margin-top: 10px;font-family: sans-serif;\">"+
                            "Retrieving token from server"+
                            "</div>"+
                        "</form>"+
                    "</div>"+
                "+</div>" +
            "</div>"+
            "</body>"+
            "</html>");
            AuthenticationWindowResult loginResult = new AuthenticationWindowResult() { Success = true, AuthCode = authCode };
            Complete?.Invoke(loginResult, new RoutedEventArgs());
        }

        private void WebView_UnsupportedUriSchemeIdentified(WebView sender, WebViewUnsupportedUriSchemeIdentifiedEventArgs args)
        {
            if (args.Uri.Scheme == "siv") { 
                args.Handled = true;
                string uri = args.Uri.ToString();
                string authCode = GetAuthCodeFromResult(uri);
                AuthCodeRetrieved?.Invoke(authCode, new RoutedEventArgs());
            }
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            OAuth2AuthenticationWindowArgs args = (OAuth2AuthenticationWindowArgs)e.Parameter;
            AuthorizeEndpoint = args.AuthorizeEndpoint;
            
            GetAuthorizationCode(args.Parameters);
        }
        private string GetAuthCodeFromResult(string result)
        {
            string query = result.Split("?")[1];
            NameValueCollection parameters = HttpUtility.ParseQueryString(query);
            string authCode = parameters.Get("code");
            string state = parameters.Get("state");
            return authCode;
        }
        private async Task GetAuthorizationCode(Dictionary<string,string> parameters)
        {
            string uri = InsertParameters(AuthorizeEndpoint, parameters);
            Uri StartUri = new Uri(uri);
            webView.Navigate(StartUri);
        }
        private string InsertParameters(string uri, Dictionary<string, string> parameters)
        {
            string result = uri;
            if (parameters.Values.Count > 0)
                result = result + "?";
            foreach(KeyValuePair<string, string> kvp in parameters)
            {
                if(kvp.Value != null)
                {
                    result = result+kvp.Key + "=" + kvp.Value+"&";
                }
            }
            result = result.Substring(0, result.Length - 1);
            return result;
        }
    }
}
