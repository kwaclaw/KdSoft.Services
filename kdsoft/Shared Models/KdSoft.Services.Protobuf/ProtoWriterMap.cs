using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace KdSoft.Services.Protobuf
{
    public class ProtoWriterMap
    {
        Dictionary<Type, IProtoWriter> writerMap;

        public ProtoWriterMap() {
            writerMap = new Dictionary<Type, IProtoWriter>();
        }

        public IProtoWriter AddWriter<TMessage, TModel>()
            where TMessage : IMessage<TMessage>, ICopyFrom<TMessage, TModel>, new()
            where TModel : class
        {
            var protoWriter = new ProtoWriter<TMessage, TModel>();
            writerMap.Add(typeof(TModel), protoWriter);
            return protoWriter;
        }

        /// <summary>
        /// Get the <see cref="IProtoWriter"/> mapped to the given type or the closest base type.
        /// </summary>
        /// <param name="key">Mapped type.</param>
        /// <returns><see cref="IProtoWriter"/> implementation.</returns>
        public IProtoWriter GetWriter(Type key) {
            IProtoWriter result = null;
            var sliceType = key;
            while (sliceType != null && !writerMap.TryGetValue(sliceType, out result))
                sliceType = sliceType.GetTypeInfo().BaseType;
            return result;
        }

        /// <summary>
        /// Get the <see cref="IProtoWriter"/> mapped to the given type.
        /// </summary>
        /// <param name="key">Mapped type.</param>
        /// <returns><see cref="IProtoWriter"/> implementation.</returns>
        public IProtoWriter GetWriterExact(Type key) {
            return writerMap[key];
        }
    }
}
