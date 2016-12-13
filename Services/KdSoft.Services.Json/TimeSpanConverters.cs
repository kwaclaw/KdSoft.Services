using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace KdSoft.Services.Json
{
    /// <summary>
    /// Custom Json converter for TimeSpans that serializes to/from an integer number of milli-seconds.
    /// </summary>
    public class TimeSpanMilliSecondConverter: JsonConverter
    {
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(TimeSpan) || objectType == typeof(TimeSpan?);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            if (reader.TokenType == JsonToken.Null) {
                return null;
            }
            var value = serializer.Deserialize<Int64>(reader);
            return TimeSpan.FromMilliseconds(value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var ts = (TimeSpan)value;
            var msec = (Int64)Math.Round(ts.TotalMilliseconds);
            serializer.Serialize(writer, msec);
        }
    }

    /// <summary>
    /// Custom Json converter for TimeSpans that converts to/from an ISO 8601 string.
    /// </summary>
    public class TimeSpanISO8601Converter: JsonConverter
    {
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(TimeSpan) || objectType == typeof(TimeSpan?);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            if (reader.TokenType == JsonToken.Null) {
                return null;
            }
            var value = serializer.Deserialize<string>(reader);
            return XmlConvert.ToTimeSpan(value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var ts = (TimeSpan)value;
            var tsString = XmlConvert.ToString(ts);
            serializer.Serialize(writer, tsString);
        }
    }

}
