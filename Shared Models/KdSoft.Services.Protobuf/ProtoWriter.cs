using Google.Protobuf;
using System.IO;

namespace KdSoft.Services.Protobuf
{
    public interface IProtoWriter
    {
        void WriteTo(object obj, Stream output);
    }

    public interface IProtoWriter<TModel>: IProtoWriter where TModel : class
    {
        void WriteTo(TModel obj, Stream output);
    }

    public interface ICopyFrom<TMessage, TModel>
          where TMessage : IMessage<TMessage>
    {
        TMessage CopyFrom(TModel model);
    }

    public class ProtoWriter<TMessage, TModel>: IProtoWriter<TModel>, IProtoWriter
        where TMessage : IMessage<TMessage>, ICopyFrom<TMessage, TModel>, new()
        where TModel : class
    {
        public void WriteTo(TModel obj, Stream output) {
            var message = new TMessage();
            message.CopyFrom(obj);
            message.WriteTo(output);
        }

        public void WriteTo(object obj, Stream output) {
            WriteTo((TModel)obj, output);
        }
    }
}
