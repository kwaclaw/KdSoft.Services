using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using KdSoft.Data.Models.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KdSoft.Services.WebApi
{
    public static class Extensions
    {
        public static string GetFullName(this ClaimsIdentity identity) {
            string result = "";

            var claim = identity.FindFirst(ClaimTypes.Surname);
            if (claim != null)
                result = claim.Value;

            claim = identity.FindFirst(ClaimTypes.GivenName);
            if (claim != null) {
                if (string.IsNullOrWhiteSpace(result))
                    result = claim.Value;
                else
                    result += ", " + claim.Value;
            }

            return result;
        }
        public static void EnsurePascalCaseKeys(this Dictionary<string, object> dict) {
            StringBuilder sb = null;
            var keys = new string[dict.Count];
            dict.Keys.CopyTo(keys, 0);
            foreach (string oldKey in keys) {
                var oldChar = oldKey[0];
                var newChar = char.ToUpper(oldChar);
                if (oldChar == newChar)
                    continue;

                if (sb == null)
                    sb = new StringBuilder(oldKey);
                else {
                    sb.Clear();
                    sb.Append(oldKey);
                }
                sb[0] = newChar;
                var newKey = sb.ToString();

                var oldVal = dict[oldKey];
                dict.Remove(oldKey);
                dict.Add(newKey, oldVal);
            }
        }

        public static Dictionary<string, object> ConvertToPascalCaseKeys(this Dictionary<string, object> dict) {
            StringBuilder sb = null;
            Dictionary<string, object> result = null;

            foreach (var oldEntry in dict) {
                var oldChar = oldEntry.Key[0];
                var newChar = char.ToUpper(oldChar);
                if (oldChar == newChar)
                    continue;

                if (result == null) {
                    result = new Dictionary<string, object>(dict.Count);
                    sb = new StringBuilder(oldEntry.Key);
                }
                else {
                    sb.Clear();
                    sb.Append(oldEntry.Key);
                }
                sb[0] = newChar;

                result.Add(sb.ToString(), oldEntry.Value);
            }

            return result ?? dict;
        }

        public static void ConvertLastRecordJObject(this SortNextFilter snf, JsonSerializer serializer) {
            var jobj = snf.LastRecord as JObject;
            if (jobj != null) {
                snf.LastRecord = jobj.ToObject<Dictionary<string, object>>(serializer);
            }
        }
    }
}
