using Google.Protobuf;
using System.IO;
using System.Reflection;
using KdSoft.Reflection;

namespace KdSoft.Services.Protobuf
{
    public interface IProtoReader
    {
        object ReadFrom(Stream input);
        object ReadFrom(ref object obj, Stream input);
    }

    public interface ICopyTo<TModel>
    {
        TModel CopyTo(TModel model);
    }

    public class ProtoReader<TMessage, TModel>: IProtoReader
        where TMessage : IMessage<TMessage>, ICopyTo<TModel>
        where TModel : class, new()
    {
        MessageParser<TMessage> parser;

        public ProtoReader() {
            var value = PropertyUtils.GetStaticPropertyValue(typeof(TMessage), "Parser") as MessageParser<TMessage>;
            if (value == null)
                throw new System.Exception("Type parameter does not have a static 'Parser' property of type 'MessageParser<T>'.");

            this.parser = value;
        }

        public object ReadFrom(Stream input) {
            var message = parser.ParseFrom(input);
            return message.CopyTo(new TModel());
        }

        public object ReadFrom(ref object obj, Stream input) {
            var message = parser.ParseFrom(input);
            return message.CopyTo((TModel)obj);
        }
    }
}
