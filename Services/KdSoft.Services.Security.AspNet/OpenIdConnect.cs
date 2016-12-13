using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using security = KdSoft.Services.Security;

namespace KdSoft.Services.Security.AspNet
{
    public static class OpenIdConnect
    {
        const string https = "https://";

        public static async Task<JObject> CheckOpenIdAuthorizationCode(
            string issuer,
            string authCode,
            string redirectUri,
            QLineAuthenticationOptions options
        ) {
            var http = new HttpClient();

            // get token endpoint from issuer's openid configuration
            string openIdConfigUrl;
            if (issuer.StartsWith(https, StringComparison.OrdinalIgnoreCase)) {
                openIdConfigUrl = string.Concat(issuer, "/.well-known/openid-configuration");
                issuer = issuer.Remove(0, https.Length);  // normalize for later
            }
            else
                openIdConfigUrl = string.Concat("https://", issuer, "/.well-known/openid-configuration");

            var openIdConfigStr = await http.GetStringAsync(openIdConfigUrl).ConfigureAwait(false);
            var openIdConfig = JObject.Parse(openIdConfigStr);
            string tokenEndpoint = (string)openIdConfig["token_endpoint"];

            var oauthCredentials = options.GetOAuthCredentials((oac) => string.Compare(oac.Issuer, issuer, true) == 0)
              .FirstOrDefault();
            if (oauthCredentials == null)
                return null;

            var contentValues = new List<KeyValuePair<string, string>>(5);
            contentValues.Add(new KeyValuePair<string, string>("code", authCode));
            contentValues.Add(new KeyValuePair<string, string>("client_id", oauthCredentials.ClientId));
            contentValues.Add(new KeyValuePair<string, string>("client_secret", oauthCredentials.ClientSecret));
            if (!string.IsNullOrEmpty(redirectUri))
                contentValues.Add(new KeyValuePair<string, string>("redirect_uri", redirectUri));
            contentValues.Add(new KeyValuePair<string, string>("grant_type", "authorization_code"));
            var content = new FormUrlEncodedContent(contentValues);

            JObject idToken = null;
            var response = await http.PostAsync(tokenEndpoint, content).ConfigureAwait(false);
            string responseStr = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var tokenResponse = JObject.Parse(responseStr);
            var idTokenEncoded = (string)tokenResponse["id_token"];
            if (idTokenEncoded != null) {
                int firstDot = idTokenEncoded.IndexOf('.');
                if (firstDot < 0)
                    return null;
                int lastDot = idTokenEncoded.LastIndexOf('.');
                if (lastDot < 0)
                    return null;
                string idTokenBase64 = idTokenEncoded.Substring(firstDot + 1, lastDot - firstDot - 1);
                //var utf8bytes = TextEncodings.Base64Url.Decode(idTokenBase64); // not an efficient implementation
                var utf8bytes = security.Utils.Base64UrlDecode(idTokenBase64);
                var idTokenStr = Encoding.UTF8.GetString(utf8bytes);
                idToken = JObject.Parse(idTokenStr);
            }

            // we do not need this currently, as we can get the email in the id token
            //JObject userInfo = null;
            //string userinfoEndpoint = (string)openIdConfig["userinfo_endpoint"];
            //var request = new HttpRequestMessage(HttpMethod.Get, userinfoEndpoint);
            //request.Headers.Authorization = new AuthenticationHeaderValue((string)tokenResponse["token_type"], (string)tokenResponse["access_token"]);
            //response = await http.SendAsync(request).ConfigureAwait(false);
            //responseStr = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            //userInfo = JObject.Parse(responseStr);

            return idToken;
        }
    }
}
