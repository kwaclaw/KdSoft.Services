using KdSoft.Data.Models.Shared.Security;
using KdSoft.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using sysClaims = System.Security.Claims;

namespace KdSoft.Services.Security.AspNet
{
    class KdSoftAuthenticationHandler: AuthenticationHandler<KdSoftAuthenticationOptions>
    {
        struct AuthenticationResult
        {
            // will be set when authentication succeeded and claims were successfully retrieved
            public AuthenticationTicket Ticket { get; set; }

            // will be set after a token is validated, but not when a token was just used to look up claims
            public JwtSecurityToken Token { get; set; }

            // the tokens valid times will be set whenever a token is passed in, even if the actual values
            // are retrieved from the cache instead of the token
            public DateTime TokenValidFrom { get; set; }
            public DateTime TokenValidTo { get; set; }

            public byte[] ClaimsId { get; set; }
        }

        IAuthenticationProvider provider;
        AuthenticationContext authContext;
        AuthenticationResult authResult = default(AuthenticationResult);

        readonly TokenValidationParameters validationParameters;
        readonly JwtSecurityTokenHandler tokenHandler;
        readonly ILogger logger;
        readonly BufferPool bufferPool;
        readonly IStringLocalizer<KdSoftAuthenticationMiddleware> localizer;

        public KdSoftAuthenticationHandler(TokenValidationParameters validationParameters, JwtSecurityTokenHandler tokenHandler, ILogger logger, IStringLocalizer<KdSoftAuthenticationMiddleware> localizer) {
            this.validationParameters = validationParameters;
            this.tokenHandler = tokenHandler;
            this.logger = logger;
            this.localizer = localizer;
            this.bufferPool = new BufferPool();
        }

        public static ClaimsIdentity CreateClaimsIdentityFromClaims(IList<Claim> claims) {
            string authType = claims.Where(cl => cl.Type == ClaimTypes.AuthType).First().Value;
            // if authentication type was set to "" then the token was revoked or logged out of
            if (string.IsNullOrEmpty(authType))
                throw new SecurityTokenValidationException("Token was revoked.");
            var claimsIdentity = new ClaimsIdentity(authType, sysClaims.ClaimTypes.Name, sysClaims.ClaimTypes.Role);
            claimsIdentity.AddClaims(claims);
            return claimsIdentity;
        }

        public static string GetTokenSignature(string tokenStr) {
            int lastDotPos = tokenStr.LastIndexOf('.');
            if (lastDotPos < 0)
                throw new SecurityTokenValidationException("Not a valid security token.");
            return tokenStr.Substring(lastDotPos + 1);
        }

        // also tries to normalize the key
        public string GetActiveDirectoryClaimsKey(string adUserName) {
            string domain, userName;
            if (Security.Utils.TryParseAdUserName(adUserName, out domain, out userName)) {
                if (domain == null)
                    domain = provider.GetDefaultADDomain();
                return string.Concat(SecurityConfig.ActiveDirectoryClaimsIdPrefix, domain.ToUpperInvariant(), "\\", userName.ToUpperInvariant());
            }
            else
                return SecurityConfig.ActiveDirectoryClaimsIdPrefix + adUserName.ToUpperInvariant();
        }

        // Make sure this is called only with the claims that should be stored in the JWT token (only user key and user name)
        JwtSecurityToken CreateAccessToken(ClaimsIdentity subject) {
            var now = DateTime.UtcNow;
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = subject,
                Issuer = Options.JwtIssuer,
                Audience = Options.JwtAudience,
                SigningCredentials = Options.JwtCredentials,
                IssuedAt = now,
                Expires = now.Add(Options.JwtLifeTime),
            };
            return (JwtSecurityToken)tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
        }

        //byte[] GetTokenKey(string tokenStr) {
        //  byte[] tokenKeyBuffer = null;
        //  try {
        //    int lastDotPos = tokenStr.LastIndexOf('.');
        //    if (lastDotPos < 0)
        //      throw new SecurityTokenValidationException("Not a valid security token.");
        //    lastDotPos++;
        //    int tokenKeyStrLen = tokenStr.Length - lastDotPos;
        //    int maxByteCount = Encoding.UTF8.GetMaxByteCount(tokenKeyStrLen);

        //    tokenKeyBuffer = bufferPool.Acquire(maxByteCount);
        //    int byteCount = Encoding.UTF8.GetBytes(tokenStr, lastDotPos, tokenKeyStrLen, tokenKeyBuffer, 0);
        //    var tokenKey = new byte[byteCount];
        //    //Array.Copy(tokenKeyBuffer, tokenKey, byteCount);
        //    Buffer.BlockCopy(tokenKeyBuffer, 0, tokenKey, 0, byteCount);
        //    return tokenKey;
        //  }
        //  finally {
        //    if (tokenKeyBuffer != null)
        //      bufferPool.Return(tokenKeyBuffer);
        //  }
        //}

        //TODO set authtype to "" for logging out or revoking cached token

        #region Authentication

        // this checks the access token, either looking the claims up in the cache, or revalidating the token;
        // if the cache does not contain an entry for the token, then the claims will be added to the cache;
        // returns the authentication ticket, and if the token was newly validated, the token;
        async Task<AuthenticationResult> CheckCustomToken(string tokenStr) {
            AuthenticationResult result = default(AuthenticationResult);

            string authType = "";
            IList<Claim> claims = null;
            string claimsIdStr = GetTokenSignature(tokenStr);
            byte[] claimsId = Encoding.UTF8.GetBytes(claimsIdStr);

            result.ClaimsId = claimsId;

            if (Options.ClaimsCache.ClaimsExist(claimsId)) {
                var claimsResult = await Options.ClaimsCache.GetClaimPropertyValuesAsync(claimsId).ConfigureAwait(false);
                // if no claims were found then they probably got removed immediately before the call
                if (claimsResult.Claims != null && claimsResult.Claims.Count > 0) {
                    claims = claimsResult.Claims;

                    var tickBytes = claimsResult.Properties[(int)PropertyOffsets.TokenValidFrom];
                    result.TokenValidFrom = new DateTime(BitConverter.ToInt64(tickBytes, 0));

                    tickBytes = claimsResult.Properties[(int)PropertyOffsets.TokenValidTo];
                    result.TokenValidTo = new DateTime(BitConverter.ToInt64(tickBytes, 0));

                    authType = claimsResult.Claims[(int)ClaimIndexes.AuthType].Value;

                    var utcNow = DateTime.UtcNow;
                    if (result.TokenValidTo < (utcNow + validationParameters.ClockSkew.Negate())) {
                        throw new SecurityTokenExpiredException("Security token has expired.");
                    }
                }
            }

            if (claims == null) { // if no claims are in the cache, revalidate token - it could have been issued from another (trusted) server
                SecurityToken secToken;
                var tokenPrincipal = tokenHandler.ValidateToken(tokenStr, validationParameters, out secToken);
                var tokenIdentity = (ClaimsIdentity)tokenPrincipal.Identity;
                var jwtToken = (JwtSecurityToken)secToken;

                var userKeyClaim = tokenIdentity.FindFirst(sysClaims.ClaimTypes.NameIdentifier);
                if (userKeyClaim == null)
                    throw new SecurityTokenValidationException("Missing user key.");
                int userKey;
                if (!Int32.TryParse(userKeyClaim.Value, out userKey))
                    throw new SecurityTokenValidationException("Invalid user key.");
                var userNameClaim = tokenIdentity.FindFirst(sysClaims.ClaimTypes.Name);
                if (userNameClaim == null)
                    throw new SecurityTokenValidationException("Missing user name.");
                authType = tokenIdentity.AuthenticationType;

                var validFrom = jwtToken != null ? jwtToken.ValidFrom : default(DateTime);
                var validTo = jwtToken != null ? jwtToken.ValidTo : default(DateTime);
                var claimsResult = await Options.ClaimsCache.RetrieveAndCacheClaimPropertiesAsync(
                  claimsId, userKey, userNameClaim.Value, authType, validFrom, validTo).ConfigureAwait(false);

                claims = claimsResult.Claims;

                result.TokenValidFrom = validFrom;
                result.TokenValidTo = validTo;
                result.Token = jwtToken;
            }

            if (claims != null && claims.Count > 0) {  // add cached claims to principal
                claims.Add(new Claim(ClaimTypes.ClaimsId, claimsIdStr, ClaimValueTypes.TokenSig));
                var claimsIdentity = CreateClaimsIdentityFromClaims(claims);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                result.Ticket = new AuthenticationTicket(claimsPrincipal, null, authType);
            }

            return result;
        }

        async Task<AuthenticationResult> CheckAuthenticatedUser(string userName, string authType) {
            AuthenticationResult result = default(AuthenticationResult);

            if (string.IsNullOrEmpty(userName))
                throw new ArgumentNullException("userName");

            // first we need a basic claims identity to create the access token (only UserKey and Name claims)
            var tokenClaims = new List<Claim>();
            tokenClaims.Add(new Claim(sysClaims.ClaimTypes.Name, userName));
            tokenClaims.Add(new Claim(ClaimTypes.AuthType, authType, sysClaims.ClaimValueTypes.String));

            var tokenIdentity = new ClaimsIdentity(tokenClaims, authType, sysClaims.ClaimTypes.Name, sysClaims.ClaimTypes.Role);

            // now we create the token and get the encoded signature (related to ClaimsId claim)
            var jwtToken = CreateAccessToken(tokenIdentity);
            string claimsIdStr = jwtToken.RawSignature;
            byte[] claimsId = Encoding.UTF8.GetBytes(claimsIdStr);

            result.ClaimsId = claimsId;

            var validFrom = jwtToken != null ? jwtToken.ValidFrom : default(DateTime);
            var validTo = jwtToken != null ? jwtToken.ValidTo : default(DateTime);
            // this returns extra claims needed by the application (based on the userKey)
            var claimsResult = await Options.ClaimsCache.RetrieveAndCacheClaimPropertiesAsync(
                claimsId, null, userName, authType, validFrom, validTo).ConfigureAwait(false);

            result.TokenValidFrom = validFrom;
            result.TokenValidTo = validTo;
            result.Token = jwtToken;

            // the ClaimsId claim is not cached itself, but it is attached to the new identity
            claimsResult.Claims.Add(new Claim(ClaimTypes.ClaimsId, claimsIdStr, ClaimValueTypes.TokenSig));
            // the finall identity has all the claims needed by the application
            var claimsIdentity = new ClaimsIdentity(claimsResult.Claims, authType, sysClaims.ClaimTypes.Name, sysClaims.ClaimTypes.Role);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            result.Ticket = new AuthenticationTicket(claimsPrincipal, null, authType);

            return result;
        }

        //http://stackoverflow.com/questions/28542141/windows-authentication-in-asp-net-5

        async Task<AuthenticationResult> CheckWindowsUser() {
            AuthenticationResult result = default(AuthenticationResult);

            var winIdentity = Context.User.Identity as WindowsIdentity;
            if (winIdentity == null || !winIdentity.IsAuthenticated)
                return result;

            string adUserName = winIdentity.Name;
            string authType = winIdentity.AuthenticationType;

            // first we check if we are already authenticated through Windows - we assume it is the primary Identity
            //switch (authenticationType) {
            //  case System.Security.Claims.AuthenticationTypes.Windows:
            //  case System.Security.Claims.AuthenticationTypes.Kerberos:
            //  case System.Security.Claims.AuthenticationTypes.Negotiate:
            //    break;
            //  default:
            //    return result;
            //}

            IList<Claim> claims = null;
            var claimsIdStr = GetActiveDirectoryClaimsKey(adUserName);
            var claimsId = Encoding.UTF8.GetBytes(claimsIdStr);

            // now let's see if we have the claims in the cache
            if (Options.ClaimsCache.ClaimsExist(claimsId)) {
                var claimsResult = await Options.ClaimsCache.GetClaimPropertyValuesAsync(claimsId).ConfigureAwait(false);
                // if no claims were found then they probably got removed immediately before the call
                if (claimsResult.Claims != null && claimsResult.Claims.Count > 0) {
                    claims = claimsResult.Claims;
                    //result.TokenValidFrom = claimsResult.TokenValidFrom;
                    //result.TokenValidTo = claimsResult.TokenValidTo;
                }
            }

            if (claims == null) {  // no claims, check if we know the user, then populate the claims cache
                string domain, userName;
                if (!Security.Utils.TryParseAdUserName(adUserName, out domain, out userName))
                    return result;
                if (domain == null)
                    domain = provider.GetDefaultADDomain();
                var adAccount = new AdAccount { Domain = domain, UserName = userName };

                var userKey = await provider.GetActiveDirectoryUserKey(adAccount).ConfigureAwait(false);

                var claimsResult = await Options.ClaimsCache.RetrieveAndCacheClaimPropertiesAsync(
                  claimsId, userKey, adUserName, authType).ConfigureAwait(false);
                claims = claimsResult.Claims;
                //result.TokenValidFrom = claimsResult.TokenValidFrom;
                //result.TokenValidTo = claimsResult.TokenValidTo;
            }

            if (claims != null && claims.Count > 0) {
                // we do not want to duplicate the Name claim
                claims.RemoveAll(cl => cl.Type == sysClaims.ClaimTypes.Name);  //TODO check if the authenticated Windows user does actually have a Name claim
                claims.Add(new Claim(ClaimTypes.ClaimsId, claimsIdStr, ClaimValueTypes.AdUserName));
                winIdentity.AddClaims(claims);
            }

            result.Ticket = new AuthenticationTicket(Context.User, null, authType);
            return result;
        }

        async Task<AuthenticationResult> CheckCustomUser(int userKey, string userName, string authType) {
            AuthenticationResult result = default(AuthenticationResult);

            if (string.IsNullOrEmpty(userName))
                throw new ArgumentNullException("userName");

            // first we need a basic claims identity to create the access token (only UserKey and Name claims)
            var tokenClaims = new List<Claim>();
            tokenClaims.Add(new Claim(sysClaims.ClaimTypes.Name, userName));
            tokenClaims.Add(new Claim(sysClaims.ClaimTypes.NameIdentifier, userKey.ToString(), sysClaims.ClaimValueTypes.Integer));
            tokenClaims.Add(new Claim(ClaimTypes.AuthType, authType, sysClaims.ClaimValueTypes.String));

            var tokenIdentity = new ClaimsIdentity(tokenClaims, authType, sysClaims.ClaimTypes.Name, sysClaims.ClaimTypes.Role);

            // now we create the token and get the encoded signature (related to ClaimsId claim)
            var jwtToken = CreateAccessToken(tokenIdentity);
            string claimsIdStr = jwtToken.RawSignature;
            byte[] claimsId = Encoding.UTF8.GetBytes(claimsIdStr);

            result.ClaimsId = claimsId;

            var validFrom = jwtToken != null ? jwtToken.ValidFrom : default(DateTime);
            var validTo = jwtToken != null ? jwtToken.ValidTo : default(DateTime);
            // this returns extra claims needed by the application (based on the userKey)
            var claimsResult = await Options.ClaimsCache.RetrieveAndCacheClaimPropertiesAsync(
                claimsId, userKey, userName, authType, validFrom, validTo).ConfigureAwait(false);

            result.TokenValidFrom = validFrom;
            result.TokenValidTo = validTo;
            result.Token = jwtToken;

            // the ClaimsId claim is not cached itself, but it is attached to the new identity
            claimsResult.Claims.Add(new Claim(ClaimTypes.ClaimsId, claimsIdStr, ClaimValueTypes.TokenSig));
            // the finall identity has all the claims needed by the application
            var claimsIdentity = new ClaimsIdentity(claimsResult.Claims, authType, sysClaims.ClaimTypes.Name, sysClaims.ClaimTypes.Role);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            result.Ticket = new AuthenticationTicket(claimsPrincipal, null, authType);

            return result;
        }

        #endregion

        async Task<AuthenticateResult> AuthenticateCoreAsync() {
            Exception error = null;
            AuthenticationResult coreAuthResult = default(AuthenticationResult);

            try {
                if (authContext.Token != null) {
                    coreAuthResult = await CheckCustomToken(authContext.Token).ConfigureAwait(false);
                }
                else if (authContext.AuthReqType == AuthenticationContext.AuthRequestType.Custom) {
                    int? userKey = await provider.ValidateUser(authContext.UserName, authContext.Password).ConfigureAwait(false);
                    if (userKey != null) {
                        coreAuthResult = await CheckCustomUser(userKey.Value, authContext.UserName, authContext.AuthReqTypeName).ConfigureAwait(false);
                    }
                }
                else if (authContext.AuthReqType == AuthenticationContext.AuthRequestType.OpenId) {
                    var idToken = await OpenIdConnect.CheckOpenIdAuthorizationCode(
                        authContext.OpenIdIssuer, authContext.OpenIdCode, authContext.OpenIdRedirectUri, Options).ConfigureAwait(false);
                    if (idToken != null) {
                        var subject = (string)idToken["sub"];
                        var issuer = (string)idToken["iss"];
                        if (subject != null) {
                            string userName = (string)idToken["email"];
                            if (string.IsNullOrEmpty(userName))
                                userName = issuer + "/" + subject;

                            // is the user also known to our security database?
                            int? userKey = await provider.GetOpenIdUserKey(issuer, subject).ConfigureAwait(false);
                            if (userKey != null) {
                                coreAuthResult = await CheckCustomUser(userKey.Value, userName, authContext.AuthReqTypeName).ConfigureAwait(false);
                            }
                            else {  // the user is not known to the database, but may still be allowed to perform some operations
                                coreAuthResult = await CheckAuthenticatedUser(userName, authContext.AuthReqTypeName).ConfigureAwait(false);
                            }
                        }
                    }
                }
                else if (authContext.AuthReqType == AuthenticationContext.AuthRequestType.Windows && !string.IsNullOrEmpty(authContext.UserName)) {
                    var adAccount = await provider.ValidateAdUser(authContext.UserName, authContext.Password).ConfigureAwait(false);
                    if (adAccount != null) {  // user authenticated
                        // is the user also known to our security database?
                        int? userKey = await provider.GetActiveDirectoryUserKey(adAccount).ConfigureAwait(false);
                        if (userKey != null) {
                            coreAuthResult = await CheckCustomUser(userKey.Value, authContext.UserName, authContext.AuthReqTypeName).ConfigureAwait(false);
                        }
                        else {  // the user is not known to the database, but may still be allowed to perform some operations
                            coreAuthResult = await CheckAuthenticatedUser(authContext.UserName, authContext.AuthReqTypeName).ConfigureAwait(false);
                        }
                    }
                }
                else {  // otherwise we always check Windows authentication
                    coreAuthResult = await CheckWindowsUser().ConfigureAwait(false);
                }
            }
            catch (Exception ex) {
                error = ex;
                logger.LogError("User validation error.", ex);
                // As we are only authenticating here, we still let the call go through to the controller action. 
                // Our controller will check if the user is authenticated or not. Why do we do this? 
                // Well, we still need some anonymous actions to be accessible for unauthenticated users.
            }

            this.authResult = coreAuthResult;

            if (error != null) {
                return AuthenticateResult.Fail(error);
            }
            else if (coreAuthResult.Ticket == null) {
                switch (authContext.AuthReqType) {
                    case AuthenticationContext.AuthRequestType.Windows:
                        return AuthenticateResult.Fail("Windows authentication failed.");
                    case AuthenticationContext.AuthRequestType.Custom:
                        return AuthenticateResult.Fail("The user name or password provided is incorrect.");
                    case AuthenticationContext.AuthRequestType.OpenId:
                        return AuthenticateResult.Fail("OpenId Connect authentication failed.");
                    default:
                        return AuthenticateResult.Fail("Authentication failed.");
                }
            }
            else {
                return AuthenticateResult.Success(coreAuthResult.Ticket);
            }
        }

        async Task<bool> ApplyHeadersAsync() {
            try {
                bool customAuthRequested = authContext.AuthReqType == AuthenticationContext.AuthRequestType.Custom;
                bool openIdAuthRequested = authContext.AuthReqType == AuthenticationContext.AuthRequestType.OpenId;
                bool adAuthRequested = authContext.AuthReqType == AuthenticationContext.AuthRequestType.Windows && !string.IsNullOrEmpty(authContext.UserName);
                // ignore call when there are no token-authentication specific headers
                if (authContext.Token == null && !customAuthRequested && !openIdAuthRequested && !adAuthRequested)
                    return true;

                // Create initial token if login credentials were passed; in this case authToken must be != null!
                if (customAuthRequested || openIdAuthRequested || adAuthRequested) {
                    if (authResult.Token == null)  // caller may have sent headers in error
                        return true;
                    var tokenString = tokenHandler.WriteToken(authResult.Token);
                    //tokenString += authResult.Token.RawSignature;  //TODO temporary workaround, remove when fixed
                    Response.Headers.AppendCommaSeparatedValues(SecurityConfig.TokenKey, tokenString, Options.JwtLifeTime.ToString());
                    return true;
                }

                // authResult.Token must be != null now because (!customAuthRequested && !openIdAuthRequested && !adAuthRequested) is true at this point;
                // therefore this.tokenValidTo and this.tokenValidFrom must have proper values

                var renewHeaderValue = Request.Headers[SecurityConfig.RenewTokenHeader];
                if (renewHeaderValue == StringValues.Empty)
                    return true;

                // Renew access token since renew header exists.
                var halfTime = TimeSpan.FromTicks((authResult.TokenValidTo - authResult.TokenValidFrom).Ticks / 2);
                var halfExpiry = authResult.TokenValidFrom + halfTime;
                if (DateTime.UtcNow < halfExpiry) {
                    string errMsg = "Must not renew access token before half of its life time has passed.";
                    Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = errMsg;
                    return true;  // not a fatal error
                }

                var userKeyClaim = authResult.Ticket.Principal.FindFirst(sysClaims.ClaimTypes.NameIdentifier);
                if (userKeyClaim == null) {
                    Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "Missing user key.";
                    goto failedGrant;  // fatal, user should not be authorized
                }
                int userKey;
                if (!int.TryParse(userKeyClaim.Value, out userKey)) {
                    Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "Invalid user key.";
                    goto failedGrant;
                }
                var userNameClaim = authResult.Ticket.Principal.FindFirst(sysClaims.ClaimTypes.Name);
                if (userNameClaim == null) {
                    Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "Missing user name.";
                    goto failedGrant;  // fatal, user should not be authorized
                }

                try {
                    var checkResult = await CheckCustomUser(userKey, userNameClaim.Value, authResult.Ticket.Principal.Identity.AuthenticationType).ConfigureAwait(false);
                    if (checkResult.Token != null) {
                        var tokenString = tokenHandler.WriteToken(checkResult.Token);
                        Response.Headers.AppendCommaSeparatedValues(SecurityConfig.TokenKey, tokenString, Options.JwtLifeTime.ToString());
                    }
                }
                catch (Exception ex) {
                    Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "User check failed.";
                    goto failedGrant;  // fatal, user should not be authorized
                }
            }
            catch (Exception ex) {
                logger.LogError("ApplyHeaders error.", ex);
                goto failedGrant;  // fatal, user should not be authorized
            }

            return true;

        failedGrant:  // force full re-validation of token on next request
            await Options.ClaimsCache.RemoveClaimsAsync(authResult.ClaimsId);
            return false;
        }

        void FinalizeResponse() {
            try {
                bool customAuthRequested = authContext.AuthReqType == AuthenticationContext.AuthRequestType.Custom;
                bool openIdAuthRequested = authContext.AuthReqType == AuthenticationContext.AuthRequestType.OpenId;
                bool windowsAuthRequested = authContext.AuthReqType == AuthenticationContext.AuthRequestType.Windows;
                bool windowsAuthenticated = false;

                // ignore call when there are no authentication specific headers
                if (authContext.Token == null && !customAuthRequested && !openIdAuthRequested && !windowsAuthRequested) {
                    var winPrincipal = Context.User as WindowsPrincipal;
                    if (winPrincipal == null)
                        return;
                    var winIdentity = winPrincipal.Identity as WindowsIdentity;
                    if (winIdentity == null || !winIdentity.IsAuthenticated)
                        return;
                    windowsAuthenticated = true;
                }

                if (authResult.Ticket == null) {
                    switch (authContext.AuthReqType) {
                        case AuthenticationContext.AuthRequestType.Windows:
                            if (string.IsNullOrEmpty(authContext.UserName)) {
                                Response.StatusCode = 401;
                                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = localizer.GetString("Windows authentication failed.");
                            }
                            else
                                Response.MakeAuthorizationErrorResponse(localizer.GetString("ActiveDirectory authentication failed."));
                            return;
                        case AuthenticationContext.AuthRequestType.Custom:
                            Response.MakeAuthorizationErrorResponse(localizer.GetString("The user name or password provided is incorrect."));
                            return;
                        case AuthenticationContext.AuthRequestType.OpenId:
                            Response.MakeAuthorizationErrorResponse(localizer.GetString("OpenId Connect authentication failed."));
                            return;
                        default:
                            if (windowsAuthenticated && Response.StatusCode == 401) {
                                Response.MakeAuthorizationErrorResponse(localizer.GetString("ActiveDirectory authenticated but not authorized."));
                                return;
                            }
                            break;
                    }
                }

                if (Response.StatusCode == 401) {
                    Response.MakeAuthorizationErrorResponse(localizer.GetString("Unauthorized."));
                }
            }
            catch (Exception ex) {
                logger.LogError("ApplyResponseChallenge error.", ex);
            }
        }

        #region Overrides

        protected override Task<AuthenticateResult> HandleAuthenticateAsync() {
            provider = Options.AuthenticationProvider;
            authContext = AuthenticationContext.FromRequestHeaders(Request);

            try {
                return AuthenticateCoreAsync();
            }
            catch (Exception ex) {
                return Task.FromResult(AuthenticateResult.Fail(ex));
            }
        }

        public override Task<bool> HandleRequestAsync() {
            bool result = false;
            if (authContext != null) {  // only if we are handling this scheme
                // if speficially requested authentication failed, then we short-cut the pipeline and return immediately
                result = authResult.Ticket == null && authContext.AuthReqType != AuthenticationContext.AuthRequestType.None;
            }
            return Task.FromResult(result);
        }

        //protected override Task<bool> HandleForbiddenAsync(ChallengeContext context) {
        //    return base.HandleForbiddenAsync(context);
        //}

        //protected override Task<bool> HandleUnauthorizedAsync(ChallengeContext context) {
        //    return base.HandleUnauthorizedAsync(context);
        //}

        protected override async Task FinishResponseAsync() {
            if (authContext == null)  // we are not handling this scheme
                return;
            try {
                bool success = authResult.Ticket != null && Context.Response.IsSuccessStatusCode();
                if (success)
                    await ApplyHeadersAsync();
                FinalizeResponse();
            }
            finally {
                authResult = default(AuthenticationResult);
                var pv = provider;
                if (pv != null) {
                    provider = null;
                    pv.Dispose();
                }
            }
        }

        #endregion
    }
}
