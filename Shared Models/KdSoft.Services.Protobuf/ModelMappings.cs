using proto = KdSoft.Services.Protobuf;
using shared = KdSoft.Data.Models.Shared;

namespace KdSoft.Services.Protobuf
{
    partial class OpStatus: proto.ICopyTo<shared.OpStatus>, proto.ICopyFrom<OpStatus, shared.OpStatus>
    {
        public OpStatus CopyFrom(shared.OpStatus o) {
            Code = o.Code;
            if (o.Description != null) Description = o.Description;
            return this;
        }

        public shared.OpStatus CopyTo(shared.OpStatus o) {
            o.Code = Code;
            o.Description = Description;
            return o;
        }
    }

    partial class ServiceError: proto.ICopyTo<shared.ServiceError>, proto.ICopyFrom<ServiceError, shared.ServiceError>
    {
        public ServiceError CopyFrom(shared.ServiceError o) {
            Code = o.Code;
            if (o.Message != null) Message = o.Message;
            return this;
        }

        public shared.ServiceError CopyTo(shared.ServiceError o) {
            o.Code = Code;
            o.Message = Message;
            return o;
        }
    }
}
