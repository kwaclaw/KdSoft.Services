using System;
#if NET451
using System.Runtime.Serialization;
using System.Security.Permissions;
#endif

namespace KdSoft.Services.Security
{
#if NET451
    [Serializable]
#endif
    public class ClaimsCacheException: Exception
    {
        public ClaimsCacheException() : base() { }

        public ClaimsCacheException(string message) : base(message) { }

        public ClaimsCacheException(string message, Exception innerException) : base(message, innerException) { }

        public ClaimsCacheException(string message, int errorCode) : base(message) {
            this.ErrorCode = errorCode;
        }

        public ClaimsCacheException(string message, int errorCode, Exception innerException)
          : base(message, innerException) {
            this.ErrorCode = errorCode;
        }

        public int ErrorCode { get; set; }

#if NET451
        protected ClaimsCacheException(SerializationInfo info, StreamingContext context) : base(info, context) {
            if (info == null)
                throw new System.ArgumentNullException("info");
            this.ErrorCode = (int)info.GetValue("ErrorCode", typeof(int));
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            if (info == null)
                throw new System.ArgumentNullException("info");
            info.AddValue("ErrorCode", this.ErrorCode);
            // MUST call through to the base class to let it save its own state
            base.GetObjectData(info, context);
        }
#endif
    }
}
