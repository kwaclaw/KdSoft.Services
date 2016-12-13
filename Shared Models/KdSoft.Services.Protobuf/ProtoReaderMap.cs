using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace KdSoft.Services.Protobuf
{
    public class ProtoReaderMap
    {
        Dictionary<Type, IProtoReader> readerMap;

        public ProtoReaderMap() {
            readerMap = new Dictionary<Type, IProtoReader>();
        }

        public IProtoReader AddReader<TMessage, TModel>()
            where TMessage : IMessage<TMessage>, ICopyTo<TModel>
            where TModel : class, new()
        {
            var protoReader = new ProtoReader<TMessage, TModel>();
            readerMap.Add(typeof(TModel), protoReader);
            return protoReader;
        }

        /// <summary>
        /// Get the <see cref="IProtoReader"/> mapped to the given type or the closest base type.
        /// </summary>
        /// <param name="key">Mapped type.</param>
        /// <returns><see cref="IProtoReader"/> implementation.</returns>
        public IProtoReader GetReader(Type key) {
            IProtoReader result = null;
            var sliceType = key;
            while (sliceType != null && !readerMap.TryGetValue(sliceType, out result))
                sliceType = sliceType.GetTypeInfo().BaseType;
            return result;
        }

        /// <summary>
        /// Get the <see cref="IProtoReader"/> mapped to the given type.
        /// </summary>
        /// <param name="key">Mapped type.</param>
        /// <returns><see cref="IProtoReader"/> implementation.</returns>
        public IProtoReader GetReaderExact(Type key) {
            return readerMap[key];
        }
    }
}
