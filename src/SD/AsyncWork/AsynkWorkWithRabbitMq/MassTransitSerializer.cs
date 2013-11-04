using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MassTransit;
using MassTransit.Serialization;
using Protoreg;

namespace AsyncWork
{
    public class MassTransitSerializer : IMessageSerializer
    {
        const string ContentTypeHeaderValue = "application/vnd.masstransit+protobuff";

        ProtoregSerializer serializer;
        public MassTransitSerializer(ProtoregSerializer serializer)
        {
            this.serializer = serializer;
        }

        public string ContentType
        {
            get { return ContentTypeHeaderValue; }
        }

        public void Deserialize(IReceiveContext context)
        {
            object obj = serializer.Deserialize(context.BodyStream);// (serializer.Deserialize(context.BodyStream) as List<Message>).First().Body;
            context.SetContentType(ContentTypeHeaderValue);
            context.SetMessageTypeConverter(new StaticMessageTypeConverter(obj));
        }

        public void Serialize<T>(Stream stream, ISendContext<T> context) where T : class
        {
            serializer.Serialize(stream, context.Message);
            context.SetContentType(ContentTypeHeaderValue);
        }
    }
}