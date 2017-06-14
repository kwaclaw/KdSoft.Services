using System;
using System.Collections.Generic;

namespace KdSoft.Services.Json
{
    /* JSON example for custom type names, see TypeNameHandling in Json.NET docs
    {
        "$type": "::OrPredicate",
        "Operands": {
            "$type": "::Predicate[]",
            "$values": [
                {
                    "$type": "::InSetFilter",
                    "Field": "TagCode",
                    "Values": [ "INCIDENT", "MANIC" ]
                },
                {
                    "$type": "::InSetFilter",
                    "Field": "TagCode",
                    "Values": [ "MISSED_MEDS" ]
                }
            ]
        }
    }   
    */
    public static class KnownTypes
    {
        public static readonly Dictionary<string, Type> Map;

        static KnownTypes() {
            Map = new Dictionary<string, Type>();
            Map.Add("::Predicate", typeof(KdSoft.Data.Models.Shared.Predicate));
            Map.Add("::NotPredicate", typeof(KdSoft.Data.Models.Shared.NotPredicate));
            Map.Add("::AndPredicate", typeof(KdSoft.Data.Models.Shared.AndPredicate));
            Map.Add("::OrPredicate", typeof(KdSoft.Data.Models.Shared.OrPredicate));
            Map.Add("::Filter", typeof(KdSoft.Data.Models.Shared.Filter));
            Map.Add("::IsNullFilter", typeof(KdSoft.Data.Models.Shared.IsNullFilter));
            Map.Add("::IsNotNullFilter", typeof(KdSoft.Data.Models.Shared.IsNotNullFilter));
            Map.Add("::InSetFilter", typeof(KdSoft.Data.Models.Shared.InSetFilter));
        }
    }
}
