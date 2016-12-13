using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace KdSoft.Services.Json
{
    /// <summary>
    /// This contract resolver will force JSON.Net to serialize a given type as the closest registered base type.
    /// This resolver can be used to avoid serializing properties in classes derived from registered base types.
    /// All one needs to do is add a base type to the resolver or at static initialization time to BaseTypes.Set.
    /// </summary>
    public class BaseTypeContractResolver: DefaultContractResolver
    {
        HashSet<Type> baseTypeSet = new HashSet<Type>();

        public BaseTypeContractResolver(params Type[] baseTypes) {
            foreach (var bt in BaseTypes.Set)
                this.baseTypeSet.Add(bt);

            foreach (var bt in baseTypes)
                this.baseTypeSet.Add(bt);
        }

        public bool AddBaseType(Type baseType) {
            return baseTypeSet.Add(baseType);
        }

        public override JsonContract ResolveContract(Type type) {
            var parent = type;
            while (!baseTypeSet.Contains(parent)) {
                parent = parent.GetTypeInfo().BaseType;
                if (parent == null) {
                    parent = type;
                    break;
                }
            }
            var result = base.ResolveContract(parent);
            return result;
        }
    }

    public class CamelCaseBaseTypeContractResolver: CamelCasePropertyNamesContractResolver
    {
        HashSet<Type> baseTypeSet = new HashSet<Type>();

        public CamelCaseBaseTypeContractResolver(params Type[] baseTypes) {
            foreach (var bt in BaseTypes.Set)
                this.baseTypeSet.Add(bt);

            foreach (var bt in baseTypes)
                this.baseTypeSet.Add(bt);
        }

        public bool AddBaseType(Type baseType) {
            return baseTypeSet.Add(baseType);
        }

        public override JsonContract ResolveContract(Type type) {
            var parent = type;
            while (!baseTypeSet.Contains(parent)) {
                parent = parent.GetTypeInfo().BaseType;
                if (parent == null) {
                    parent = type;
                    break;
                }
            }
            var result = base.ResolveContract(parent);
            return result;
        }
    }
}
