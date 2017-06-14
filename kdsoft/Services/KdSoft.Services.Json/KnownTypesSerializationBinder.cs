using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace KdSoft.Services.Json
{
    /// <summary>
    /// Maps known custom type names to CLR types and back. Used to decouple NewtonSoft.Json from .NET type system.
    /// Also handles simple array types of known types, does not handle multi-dimentionsal arrays or arrays of arrays.
    /// </summary>
    public class KnownTypesSerializationBinder: DefaultSerializationBinder
    {
        Dictionary<string, Type> nameTypeMap;
        Dictionary<Type, string> typeNameMap;
        const string arrayPostFix = "[]";

        public KnownTypesSerializationBinder(Dictionary<string, Type> knownTypes) {
            this.nameTypeMap = knownTypes;
            typeNameMap = new Dictionary<Type, string>(knownTypes.Count);
            foreach (var pair in knownTypes) {
                typeNameMap.Add(pair.Value, pair.Key);
            }
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName) {
            assemblyName = null;
            string name;
            if (serializedType.IsArray) {
                if (typeNameMap.TryGetValue(serializedType.GetElementType(), out name))
                    typeName = name + "[]";
                else
                    typeName = null;
            }
            else if (typeNameMap.TryGetValue(serializedType, out name))
                typeName = name;
            else
                typeName = null;

            if (typeName == null) {
                base.BindToName(serializedType, out assemblyName, out typeName);
            }
        }

        public override Type BindToType(string assemblyName, string typeName) {
            bool isArray = typeName.EndsWith(arrayPostFix);
            if (isArray)
                typeName = typeName.Substring(0, typeName.Length - arrayPostFix.Length);
            Type result;
            if (nameTypeMap.TryGetValue(typeName, out result)) {
                return isArray ? result.MakeArrayType() : result;
            }
            else
                return base.BindToType(assemblyName, typeName);
        }
    }
}
