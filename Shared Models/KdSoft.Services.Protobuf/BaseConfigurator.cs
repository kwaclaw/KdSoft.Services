using Google.Protobuf;
using shared = KdSoft.Data.Models.Shared;

namespace KdSoft.Services.Protobuf
{
    public abstract class BaseConfigurator
    {
        protected readonly ProtoWriterMap writerMap;
        protected readonly ProtoReaderMap readerMap;

        public BaseConfigurator(ProtoWriterMap writerMap, ProtoReaderMap readerMap) {
            this.writerMap = writerMap;
            this.readerMap = readerMap;
        }

        protected void Add<TMessage, TModel>()
            where TMessage : IMessage<TMessage>, ICopyTo<TModel>, ICopyFrom<TMessage, TModel>, new()
            where TModel : class, new()
        {
            writerMap.AddWriter<TMessage, TModel>();
            readerMap.AddReader<TMessage, TModel>();
        }

        protected void AddWriter<TMessage, TModel>()
            where TMessage : IMessage<TMessage>, ICopyFrom<TMessage, TModel>, new()
            where TModel : class
        {
            writerMap.AddWriter<TMessage, TModel>();
        }

        protected void AddReader<TMessage, TModel>()
            where TMessage : IMessage<TMessage>, ICopyTo<TModel>
            where TModel : class, new()
        {
            readerMap.AddReader<TMessage, TModel>();
        }

        protected void Add<TMessage, TWriterModel, TReaderModel>()
            where TMessage : IMessage<TMessage>, ICopyTo<TReaderModel>, ICopyFrom<TMessage, TWriterModel>, new()
            where TWriterModel : class
            where TReaderModel : class, new()
        {
            writerMap.AddWriter<TMessage, TWriterModel>();
            readerMap.AddReader<TMessage, TReaderModel>();
        }

        public virtual void Configure() {
            // NOTE: Basic struct types are mapped in BaseConverters class

            // Service Models
            Add<OpStatus, shared.OpStatus>();
            Add<ServiceError, shared.ServiceError>();
        }
    }
}
