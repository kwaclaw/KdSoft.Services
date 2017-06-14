using KdSoft.Services.Protobuf;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System;
using System.Threading.Tasks;

namespace KdSoft.Services.WebApi
{
    public class ProtobufFormatterOptions
    {
        public static readonly MediaTypeHeaderValue MediaType = new MediaTypeHeaderValue("application/x-protobuf");
    }

    public class ProtobufOutputFormatter: OutputFormatter
    {
        ProtoWriterMap writerMap;

        public ProtobufOutputFormatter() {
            SupportedMediaTypes.Add(ProtobufFormatterOptions.MediaType);
            writerMap = new ProtoWriterMap();
        }

        public ProtoWriterMap WriterMap { get { return writerMap; } }

        static bool CanWriteTypeCore(Type type) {
            return true;
        }

        protected override bool CanWriteType(Type type) {
            bool result = CanWriteTypeCore(type);
            if (result) {
                var writer = writerMap.GetWriter(type);
                if (writer == null)
                    result = false;
            }
            return result;
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context) {
            var tcs = new TaskCompletionSource<object>();

            try {
                // the type passed to serialization might be more derived than the type publicly registered,
                // so we also check if we have a parent type serializer that is registered, if no exact match is found
                var writer = writerMap.GetWriter(context.ObjectType);
                writer.WriteTo(context.Object, context.HttpContext.Response.Body);
                tcs.SetResult(null);
            }
            catch (Exception ex) {
                tcs.SetException(ex);
            }

            return tcs.Task;
        }
    }

    public class ProtobufInputFormatter: InputFormatter
    {
        ProtoReaderMap readerMap;

        public ProtobufInputFormatter() {
            SupportedMediaTypes.Add(ProtobufFormatterOptions.MediaType);
            readerMap = new ProtoReaderMap();
        }

        public ProtoReaderMap ReaderMap { get { return readerMap; } }

        static bool CanReadTypeCore(Type type) {
            return true;
        }

        protected override bool CanReadType(Type type) {
            bool result = CanReadTypeCore(type);
            if (result) {
                var reader = readerMap.GetReaderExact(type);
                if (reader == null)
                    result = false;
            }
            return result;
        }

        public override Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context) {
            var tcs = new TaskCompletionSource<InputFormatterResult>();

            try {
                // we want to deserialize the exact type that we expecting
                var reader = readerMap.GetReaderExact(context.ModelType);
                object result = reader.ReadFrom(context.HttpContext.Request.Body);

                tcs.SetResult(InputFormatterResult.Success(result));
            }
            catch (Exception ex) {
                tcs.SetResult(InputFormatterResult.Failure());
                //tcs.SetException(ex);
            }

            return tcs.Task;
        }
    }

}
