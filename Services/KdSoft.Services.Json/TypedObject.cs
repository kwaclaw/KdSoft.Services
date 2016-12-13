using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace KdSoft.Services.Json
{
    public static class TypedObject
    {
        public static readonly Dictionary<string, TypedObjectActivator> ActivatorMap = new Dictionary<string, TypedObjectActivator>();

        // For simple objects, with properties of simple types
        public static object Convert(JObject jObject, string objType) {
            TypedObjectActivator activator = null;
            ActivatorMap.TryGetValue(objType, out activator);
            if (activator == null) {
                string msg = string.Format("Cannot instantiate type '{0}'.", objType);
                throw new InvalidOperationException(msg);
            }
            return activator(jObject, GetProperty);
        }

        public static object GetProperty(JObject jObject, string propertyName) {
            var jtoken = jObject.GetValue(propertyName);
            object result = null;
            if (jtoken != null)
                result = jtoken.ToObject(typeof(object));
            return result;
        }
    }

    public delegate object JsonPropertyGetter(JObject instance, string propertyName);

    public delegate object TypedObjectActivator(JObject element, JsonPropertyGetter getField);
}
