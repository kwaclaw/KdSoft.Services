using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace KdSoft.Services.Security.AspNet
{
    static class Utils
    {
        static readonly Regex lineBreaksRegex;

        static Utils() {
            lineBreaksRegex = new Regex(@"\r\n?|\n", RegexOptions.Compiled);
        }

        public static bool TryExtractNameValue(string nameValuePair, out KeyValuePair<string, string> result, char sep = '=') {
            var parts = nameValuePair.Split('=');
            if (parts.Length != 2) {
                result = default(KeyValuePair<string, string>);
                return false;
            }
            result = new KeyValuePair<string, string>(parts[0].Trim(), parts[1].Trim());
            return true;
        }

        public static void MakeAuthorizationErrorResponse(this HttpResponse response, string reason) {
            response.Headers.AppendCommaSeparatedValues(SecurityConfig.AuthOptionsHeader, AuthenticationContext.AuthRequestTypeNames);
            response.StatusCode = 471;  // setting StatusCode later would reset ReasonPhrase
            if (!string.IsNullOrEmpty(reason)) {
                if (lineBreaksRegex.IsMatch(reason))
                    throw new ArgumentException("Reason phrase must not contain line breaks.", "reason");
                response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = reason;
            }
        }

        public static bool IsSuccessStatusCode(this HttpResponse response) {
            return response.StatusCode >= 200 && response.StatusCode <= 299;
        }
    }
}
