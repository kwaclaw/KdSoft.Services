using System;

namespace KdSoft.Data
{
#if NET451
    [Serializable]
#endif
    public class DataIntegrityException: Exception
    {
        public DataIntegrityException() : base() { }

        public DataIntegrityException(string message) : base(message) { }

        public DataIntegrityException(string message, Exception innerException) : base(message, innerException) { }
    }
}
