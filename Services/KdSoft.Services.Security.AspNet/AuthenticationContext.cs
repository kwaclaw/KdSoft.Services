using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using security = KdSoft.Services.Security;

namespace KdSoft.Services.Security.AspNet
{
    class AuthenticationContext
    {
        public enum AuthRequestType
        {
            None,
            Custom,
            Windows,
            OpenId
        }

        public AuthRequestType AuthReqType { get; set; }
        public string Token { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string OpenIdCode { get; set; }
        public string OpenIdIssuer { get; set; }
        public string OpenIdRedirectUri { get; set; }

        public static string[] AuthRequestTypeNames { get; private set; }

        static AuthenticationContext() {
            var arTypes = ((AuthRequestType[])Enum.GetValues(typeof(AuthRequestType)));
            var arTypeNames = new string[arTypes.Length - 1];
            int nameIndx = 0;
            for (int indx = 0; indx < arTypes.Length; indx++) {
                var arType = arTypes[indx];
                if (arType != AuthRequestType.None)  // we ignore "None"
                    arTypeNames[nameIndx++] = GetAuthReqTypeName(arType);
            }
            AuthRequestTypeNames = arTypeNames;
        }

        static bool AuthReqTypeMatches(string authType, string test) {
            return string.Compare(authType, test, true) == 0;
        }

        public static bool IsValidAuthRequestTypeName(string test) {
            for (int indx = 0; indx < AuthRequestTypeNames.Length; indx++) {
                if (AuthReqTypeMatches(AuthRequestTypeNames[indx], test))
                    return true;
            }
            return false;
        }

        public static string GetAuthReqTypeName(AuthRequestType authReqType) {
            switch (authReqType) {
                case AuthRequestType.Custom:
                    return SecurityConfig.AuthCustom;
                case AuthRequestType.Windows:
                    return SecurityConfig.AuthWindows;
                case AuthRequestType.OpenId:
                    return SecurityConfig.AuthOpenId;
                default:
                    return "";
            }
        }

        public string AuthReqTypeName {
            get { return GetAuthReqTypeName(AuthReqType); }
        }

        public static AuthenticationContext FromRequestHeaders(HttpRequest request) {
            var result = new AuthenticationContext();
            result.AuthReqType = AuthRequestType.None;

            var authzHeaderValues = request.Headers["Authorization"];
            var kdSoftAuthValue = authzHeaderValues
                .FirstOrDefault(h => h.Length > security.AuthenticationSchemes.KdSoft.Length && h.StartsWith(security.AuthenticationSchemes.KdSoft, StringComparison.OrdinalIgnoreCase));
            if (kdSoftAuthValue != null && char.IsWhiteSpace((char)kdSoftAuthValue[Services.Security.AuthenticationSchemes.KdSoft.Length])) {
                result.Token = kdSoftAuthValue.Substring(Services.Security.AuthenticationSchemes.KdSoft.Length + 1).Trim();
            }

            var kdSoftAuthzHeaderValues = request.Headers.GetCommaSeparatedValues(SecurityConfig.AuthHeader);
            if (kdSoftAuthzHeaderValues != null && kdSoftAuthzHeaderValues.Length > 0) {

                if (string.Compare(kdSoftAuthzHeaderValues[0], SecurityConfig.AuthCustom, true) == 0) {
                    if (kdSoftAuthzHeaderValues.Length < 3)
                        goto invalidHeader;
                    KeyValuePair<string, string> nameValue;
                    string uid = null;
                    string pwd = null;

                    for (int indx = 1; indx < kdSoftAuthzHeaderValues.Length; indx++) {
                        if (!Utils.TryExtractNameValue(kdSoftAuthzHeaderValues[indx], out nameValue))
                            goto invalidHeader;
                        if (string.Compare(nameValue.Key, SecurityConfig.AuthUid, true) == 0) {
                            uid = nameValue.Value;
                        }
                        else if (string.Compare(nameValue.Key, SecurityConfig.AuthPwd, true) == 0) {
                            pwd = nameValue.Value;
                        }
                        else
                            goto invalidHeader;
                    }
                    if (uid == null || pwd == null)
                        goto invalidHeader;

                    result.UserName = uid;
                    result.Password = pwd;
                    result.AuthReqType = AuthRequestType.Custom;

                invalidHeader:
                    ;  // skip assigning fields when header values are invalid
                }

                if (string.Compare(kdSoftAuthzHeaderValues[0], SecurityConfig.AuthOpenId, true) == 0) {
                    if (kdSoftAuthzHeaderValues.Length < 4)
                        goto invalidHeader;
                    KeyValuePair<string, string> nameValue;
                    string issuer = null;
                    string authCode = null;
                    string redirectUri = null;

                    for (int indx = 1; indx < kdSoftAuthzHeaderValues.Length; indx++) {
                        if (!Utils.TryExtractNameValue(kdSoftAuthzHeaderValues[indx], out nameValue))
                            goto invalidHeader;
                        if (string.Compare(nameValue.Key, SecurityConfig.AuthOpenIdIssuer, true) == 0) {
                            issuer = nameValue.Value;
                        }
                        else if (string.Compare(nameValue.Key, SecurityConfig.AuthOpenIdCode, true) == 0) {
                            authCode = nameValue.Value;
                        }
                        else if (string.Compare(nameValue.Key, SecurityConfig.AuthOpenIdRedirectUri, true) == 0) {
                            redirectUri = nameValue.Value;
                        }
                        else
                            goto invalidHeader;
                    }
                    if (issuer == null || authCode == null || redirectUri == null)
                        goto invalidHeader;

                    result.OpenIdIssuer = WebUtility.UrlDecode(issuer);
                    result.OpenIdCode = authCode;
                    result.OpenIdRedirectUri = WebUtility.UrlDecode(redirectUri);
                    result.AuthReqType = AuthRequestType.OpenId;

                invalidHeader:
                    ;  // skip assigning fields when header values are invalid
                }

                else if (string.Compare(kdSoftAuthzHeaderValues[0], SecurityConfig.AuthWindows, true) == 0) {
                    if (kdSoftAuthzHeaderValues.Length == 1)
                        goto endHeader;
                    if (kdSoftAuthzHeaderValues.Length < 3)
                        goto invalidHeader;

                    KeyValuePair<string, string> nameValue;
                    string uid = null;
                    string pwd = null;

                    for (int indx = 1; indx < kdSoftAuthzHeaderValues.Length; indx++) {
                        if (!Utils.TryExtractNameValue(kdSoftAuthzHeaderValues[indx], out nameValue))
                            goto invalidHeader;
                        if (string.Compare(nameValue.Key, SecurityConfig.AuthUid, true) == 0) {
                            uid = nameValue.Value;
                        }
                        else if (string.Compare(nameValue.Key, SecurityConfig.AuthPwd, true) == 0) {
                            pwd = nameValue.Value;
                        }
                        else
                            goto invalidHeader;
                    }
                    if (uid == null || pwd == null)
                        goto invalidHeader;

                    result.Password = pwd;
                    result.UserName = uid;
                endHeader:
                    result.AuthReqType = AuthRequestType.Windows;

                invalidHeader:
                    ;  // skip assigning fields when header values are invalid
                }
            }
            return result;
        }
    }
}
