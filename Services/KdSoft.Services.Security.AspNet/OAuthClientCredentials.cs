
namespace KdSoft.Services.Security.AspNet
{
    public class OAuthClientCredentials
    {
        public OAuthClientCredentials(string issuer, string clientId, string clientSecret) {
            this.Issuer = issuer;
            this.ClientId = clientId;
            this.ClientSecret = clientSecret;
        }

        // Issuer must not start with "https://" (to have a normalized value)
        public string Issuer { get; private set; }
        public string ClientId { get; private set; }
        public string ClientSecret { get; private set; }
    }
}
