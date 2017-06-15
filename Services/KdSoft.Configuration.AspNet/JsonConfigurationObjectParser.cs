using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KdSoft.Configuration.AspNet
{
    /// <summary>
    /// Helper class to parse a <see cref="JObject"/> instance into an <c>IDictionary&lt;string, string&gt;</c>.
    /// </summary>
    public class JsonConfigurationObjectParser
    {
        readonly IDictionary<string, string> data = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        readonly Stack<string> context = new Stack<string>();
        string currentPath;

        /// <summary>
        /// Parses a <see cref="JObject"/> instance into an <c>IDictionary&lt;string, string&gt;</c>.
        /// </summary>
        /// <param name="jsonObj"><see cref="JObject"/> instance to parse.</param>
        /// <returns>Dictionary instance.</returns>
        public IDictionary<string, string> Parse(JObject jsonObj) {
            data.Clear();
            VisitJObject(jsonObj);
            return data;
        }

        void VisitJObject(JObject jObject) {
            foreach (var property in jObject.Properties()) {
                EnterContext(property.Name);
                VisitProperty(property);
                ExitContext();
            }
        }

        void VisitProperty(JProperty property) {
            VisitToken(property.Value);
        }

        void VisitToken(JToken token) {
            switch (token.Type) {
                case JTokenType.Object:
                    VisitJObject(token.Value<JObject>());
                    break;

                case JTokenType.Array:
                    VisitArray(token.Value<JArray>());
                    break;

                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.String:
                case JTokenType.Boolean:
                case JTokenType.Bytes:
                case JTokenType.Raw:
                case JTokenType.Null:
                    VisitPrimitive(token);
                    break;

                default:
                    var errFormat = "Unsupported JSON token '{0}' was found.";
                    throw new FormatException(string.Format(errFormat, token.Type));
            }
        }

        void VisitArray(JArray array) {
            for (int index = 0; index < array.Count; index++) {
                EnterContext(index.ToString());
                VisitToken(array[index]);
                ExitContext();
            }
        }

        void VisitPrimitive(JToken data) {
            var key = currentPath;

            if (this.data.ContainsKey(key)) {
                throw new FormatException(string.Format("A duplicate key '{0}' was found.", key));
            }
            this.data[key] = data.ToString();
        }

        void EnterContext(string context) {
            this.context.Push(context);
            currentPath = ConfigurationPath.Combine(this.context.Reverse());
        }

        void ExitContext() {
            context.Pop();
            currentPath = ConfigurationPath.Combine(context.Reverse());
        }
    }
}
