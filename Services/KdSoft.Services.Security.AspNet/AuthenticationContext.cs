using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using security = KdSoft.Services.Security;

namespace KdSoft.Services.Security.AspNet
{
    class AuthenticationContext
    {
        public enum AuthRequestType
        {
            None,
            QLine,
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
                case AuthRequestType.QLine:
                    return SecurityConfig.QlineAuthBasic;
                case AuthRequestType.Windows:
                    return SecurityConfig.QlineAuthWindows;
                case AuthRequestType.OpenId:
                    return SecurityConfig.QlineAuthOpenId;
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
            var qlineAuthValue = authzHeaderValues
                .Where(h => h.Length > security.AuthenticationSchemes.QLine.Length && h.StartsWith(security.AuthenticationSchemes.QLine, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();
            if (qlineAuthValue != null && char.IsWhiteSpace((char)qlineAuthValue[Services.Security.AuthenticationSchemes.QLine.Length])) {
                result.Token = qlineAuthValue.Substring(Services.Security.AuthenticationSchemes.QLine.Length + 1).Trim();
            }

            var qlineAuthzHeaderValues = request.Headers.GetCommaSeparatedValues(SecurityConfig.QlineAuthHeader);
            if (qlineAuthzHeaderValues != null && qlineAuthzHeaderValues.Length > 0) {

                if (string.Compare(qlineAuthzHeaderValues[0], SecurityConfig.QlineAuthBasic, true) == 0) {
                    if (qlineAuthzHeaderValues.Length < 3)
                        goto invalidHeader;
                    KeyValuePair<string, string> nameValue;
                    string uid = null;
                    string pwd = null;

                    for (int indx = 1; indx < qlineAuthzHeaderValues.Length; indx++) {
                        if (!Utils.TryExtractNameValue(qlineAuthzHeaderValues[indx], out nameValue))
                            goto invalidHeader;
                        if (string.Compare(nameValue.Key, SecurityConfig.QlineAuthUid, true) == 0) {
                            uid = nameValue.Value;
                        }
                        else if (string.Compare(nameValue.Key, SecurityConfig.QlineAuthPwd, true) == 0) {
                            pwd = nameValue.Value;
                        }
                        else
                            goto invalidHeader;
                    }
                    if (uid == null || pwd == null)
                        goto invalidHeader;

                    result.UserName = uid;
                    result.Password = pwd;
                    result.AuthReqType = AuthRequestType.QLine;

                invalidHeader:
                    ;  // skip assigning fields when header values are invalid
                }

                if (string.Compare(qlineAuthzHeaderValues[0], SecurityConfig.QlineAuthOpenId, true) == 0) {
                    if (qlineAuthzHeaderValues.Length < 4)
                        goto invalidHeader;
                    KeyValuePair<string, string> nameValue;
                    string issuer = null;
                    string authCode = null;
                    string redirectUri = null;

                    for (int indx = 1; indx < qlineAuthzHeaderValues.Length; indx++) {
                        if (!Utils.TryExtractNameValue(qlineAuthzHeaderValues[indx], out nameValue))
                            goto invalidHeader;
                        if (string.Compare(nameValue.Key, SecurityConfig.QlineAuthOpenIdIssuer, true) == 0) {
                            issuer = nameValue.Value;
                        }
                        else if (string.Compare(nameValue.Key, SecurityConfig.QlineAuthOpenIdCode, true) == 0) {
                            authCode = nameValue.Value;
                        }
                        else if (string.Compare(nameValue.Key, SecurityConfig.QlineAuthOpenIdRedirectUri, true) == 0) {
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

                else if (string.Compare(qlineAuthzHeaderValues[0], SecurityConfig.QlineAuthWindows, true) == 0) {
                    if (qlineAuthzHeaderValues.Length == 1)
                        goto endHeader;
                    if (qlineAuthzHeaderValues.Length < 3)
                        goto invalidHeader;

                    KeyValuePair<string, string> nameValue;
                    string uid = null;
                    string pwd = null;

                    for (int indx = 1; indx < qlineAuthzHeaderValues.Length; indx++) {
                        if (!Utils.TryExtractNameValue(qlineAuthzHeaderValues[indx], out nameValue))
                            goto invalidHeader;
                        if (string.Compare(nameValue.Key, SecurityConfig.QlineAuthUid, true) == 0) {
                            uid = nameValue.Value;
                        }
                        else if (string.Compare(nameValue.Key, SecurityConfig.QlineAuthPwd, true) == 0) {
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
