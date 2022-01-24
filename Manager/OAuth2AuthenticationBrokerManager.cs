using OAuth2AuthenticationBroker.Enums;
using OAuth2AuthenticationBroker.Window;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using System.Net.Http.Headers;
using System.Net.Http;
using Newtonsoft.Json;
using OAuth2AuthenticationBroker.Models;
using OAuth2AuthenticationBroker.Functions;
using System.Reflection;

namespace OAuth2AuthenticationBroker.Manager
{
    /// <summary>
    /// Manages the authentication process
    /// </summary>
    public class OAuth2AuthenticationBrokerManager
    {
        private static AppWindow AppWindow;
        /// <summary>
        /// The Resource you are wanting to access
        /// </summary>
        public string Resource { get; set; }
        /// <summary>
        /// The Uri that you want to be redirected to after the authentication
        /// </summary>
        public string RedirectUri { get; set; }
        /// <summary>
        /// The Client Id you will use to make the request to the auth server
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// The Client Secret you will use to make the request to the auth server if provided
        /// </summary>
        public string ClientSecret { get; set; }
        /// <summary>
        /// The endpoint for the authorize portion of the request
        /// </summary>
        public string AuthorizeEndpoint { get; set; }
        /// <summary>
        /// The endpoint for the token retrieval portion of the request
        /// </summary>
        public string TokenEndpoint { get; set; }
        /// <summary>
        /// The endpoint for the user info portion of the request
        /// </summary>
        public string UserInfoEndpoint { get; set; }
        /// <summary>
        /// The title of the window used for login
        /// </summary>
        public string WindowTitle { get; set; }
        /// <summary>
        /// Set whether the code challenge will be encrypted
        /// </summary>
        public OAuthCodeChallengeMethod ChallengeMethod { get; set; }
        /// <summary>
        /// Sets the response type for the authentication
        /// </summary>
        public OAuthResponseType ResponseType { get; set; }
        /// <summary>
        /// The scopes you are requesting
        /// </summary>
        public List<string> Scopes { get; set; }
        /// <summary>
        /// The result of the request
        /// </summary>
        private OAuthLoginResult Result { get; set; }
        /// <summary>
        /// Is true if the broker is currently running
        /// </summary>
        private bool Running { get; set; }
        private static event RoutedEventHandler AuthComplete;
        private string CodeVerifier { get; set; }
        private string CodeChallenge { get; set; }
        private string State { get; set; }
        public static string AgentName = "OAuth2AuthenticatonBroker";
        public OAuth2AuthenticationBrokerManager()
        {
            ChallengeMethod = OAuthCodeChallengeMethod.Plain;
            WindowTitle = "SSO Login";
        }
        /// <summary>
        /// Starts the authentication process by opening a new window which will show the authentication page
        /// </summary>
        /// <returns>The result of the authentication process</returns>
        public async Task<OAuthLoginResult> StartInNewWindowAsync()
        {
            AuthComplete += OAuth2AuthenticationBrokerManager_AuthComplete;
            State = StateGenerator.GenerateState();
            GenerateCodeChallenge();
            AppWindow = await AppWindow.TryCreateAsync();
            AppWindow.Title = WindowTitle;
            AppWindow.RequestSize(new Windows.Foundation.Size(500, 900));
            Frame frame = new Frame();
            Dictionary<string, string> parameters = GetParameters();
            frame.Navigate(typeof(AuthenticationWindow), new OAuth2AuthenticationWindowArgs() { Parameters = parameters, AuthorizeEndpoint = AuthorizeEndpoint }) ;
            ElementCompositionPreview.SetAppWindowContent(AppWindow, frame);
            if (await AppWindow.TryShowAsync())
            {
                Running = true;
                AppWindow.Closed += AppWindow_Closed;
                AuthenticationWindow.Complete += OAuth2AuthenticationWindow_Complete;
                while (Running)
                {
                    await Task.Delay(50);
                }
                return Result;
            }
            return null;
        }

        private void OAuth2AuthenticationBrokerManager_AuthComplete(object sender, RoutedEventArgs e)
        {
            
        }

        private Dictionary<string, string> GetParameters()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("response_type", ResponseType.ToString().ToLower());
            parameters.Add("client_id", ClientId);
            parameters.Add("client_secret", ClientSecret);
            parameters.Add("code_challenge_method", ChallengeMethod.ToString().ToUpper());
            parameters.Add("code_challenge", CodeChallenge);
            parameters.Add("state", State);
            parameters.Add("resource", Resource);
            parameters.Add("redirect_uri", RedirectUri);
            string scopeString = "";
            foreach (string scope in Scopes)
            {
                scopeString = scopeString + scope + ",";
            }
            if (scopeString.Length > 0)
            {
                scopeString = scopeString.Substring(0, scopeString.Length - 1);
                parameters.Add("scope", scopeString);
            }
            return parameters;
        }
        private void GenerateCodeChallenge()
        {
            CodeVerifier = Guid.NewGuid().ToString();
            if (ChallengeMethod == OAuthCodeChallengeMethod.S256)
            {
                CodeChallenge = HashGenerator.ComputeStringToSha256Hash(CodeVerifier);
            }
            else
                CodeChallenge = CodeVerifier;
        }

        private async Task<OAuthLoginResult> GetTokenFromAuthCode(string authCode)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(TokenEndpoint);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("utf-8"));
                httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(AgentName, Assembly.GetExecutingAssembly().GetName().Version.ToString()));

                Dictionary<string, string> formContent = new Dictionary<string, string>();
                formContent.Add("client_id", ClientId);
                formContent.Add("client_secret", ClientSecret);
                formContent.Add("grant_type", "authorization_code");
                formContent.Add("code_verifier", CodeVerifier);
                formContent.Add("code", authCode);
                formContent.Add("audience", "SecureImageViewer");
                FormUrlEncodedContent formUrlEncodedContent = new FormUrlEncodedContent(formContent);
                HttpContent httpContent = formUrlEncodedContent;
                HttpResponseMessage response = await httpClient.PostAsync("", httpContent);
                string jsonResponse = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    //do something with json response here
                    OAuthResult tokenResult = JsonConvert.DeserializeObject<OAuthResult>(jsonResponse);
                    return new OAuthLoginResult() { Identity = tokenResult, Success = true };
                }
                else
                {
                    OAuthResponseError error = JsonConvert.DeserializeObject<OAuthResponseError>(jsonResponse);
                    return new OAuthLoginResult() { Error = error, Success = false };
                }
            }
        }
        public async Task<T> GetUserInfo<T>(string accessToken)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(UserInfoEndpoint);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("utf-8"));
                httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(AgentName, Assembly.GetExecutingAssembly().GetName().Version.ToString()));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
                HttpContent httpContent = new StringContent("");
                HttpResponseMessage response = await httpClient.GetAsync("");
                string jsonResponse = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    //do something with json response here
                    return JsonConvert.DeserializeObject<T>(jsonResponse);
                }
                else
                    return default(T);
            }
        }

        private void AppWindow_Closed(AppWindow sender, AppWindowClosedEventArgs args)
        {
            AuthenticationWindow.Complete -= OAuth2AuthenticationWindow_Complete;
            AppWindow.ClearAllPersistedState();
        }

        private async void OAuth2AuthenticationWindow_Complete(object sender, RoutedEventArgs e)
        {
            AuthenticationWindowResult windowResult = (AuthenticationWindowResult)sender;
            string authCode = windowResult.AuthCode;
            Result = await GetTokenFromAuthCode(authCode);
            AppWindow.CloseAsync();
            AppWindow.ClearAllPersistedState();           
            Running = false;
        }
    }
}
