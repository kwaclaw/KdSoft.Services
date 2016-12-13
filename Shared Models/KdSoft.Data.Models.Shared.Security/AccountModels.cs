namespace KdSoft.Data.Models.Shared.Security
{
    public class ChangePasswordModel
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class RegisterModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoggedInAs
    {
        public string UserName { get; set; }
        public bool IsAuthenticated { get; set; }
    }

    public class OpenIdAuthorization
    {
        public string Issuer { get; set; }
        public string AuthorizationCode { get; set; }
        public string RedirectUri { get; set; }
    }

    public class OpenIdAccount
    {
        public string Issuer { get; set; }
        public string Subject { get; set; }
        public string Email { get; set; }
    }

    public class OAuthClientAppId
    {
        public string Issuer { get; set; }
        public string ClientId { get; set; }
        public string Application { get; set; }
    }
}
