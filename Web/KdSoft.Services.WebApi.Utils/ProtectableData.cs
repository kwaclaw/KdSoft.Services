using System.Text;
using Microsoft.AspNetCore.DataProtection;

namespace KdSoft.Services.WebApi
{

    /// <summary>
    /// Utility class to encrypt and decrypt data based on <see cref="IDataProtector"/> implementations.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ProtectableData<T>
    {
        public string Name { get; set; }
        public T RawValue { get; set; }
        public byte[] ProtectedValue { get; set; }

        public bool IsEmpty {
            get { return object.Equals(RawValue, default(T)) && (ProtectedValue == null || ProtectedValue.Length == 0); }
        }

        protected abstract byte[] ToBytes(T value);
        protected abstract T ToValue(byte[] bytes);

        /// <summary>
        /// Extracts stored value (protected or not) and protects it in the process, if necessary.
        /// </summary>
        /// <param name="dprProvider"><see cref="IDataProtectionProvider"/> instance.</param>
        /// <returns>Value tuple containing extracted value and indicator if instance had been modified in the process.</returns>
        /// <remarks>Call only once after construction/initialization of instance! Not idempotent!</remarks>
        public virtual (T Value, bool Modified) ExtractValue(IDataProtectionProvider dprProvider) {
            bool needToSave = false;
            T data = default(T);
            var protector = dprProvider.CreateProtector(Name);

            if (!object.Equals(RawValue, default(T))) {
                data = RawValue;
                ProtectedValue = protector.Protect(ToBytes(data));
                RawValue = default(T);  // clear RawValue, we only use it once
                needToSave = true;
            }
            else if (ProtectedValue != null && ProtectedValue.Length > 0) {
                byte[] unprotectedValue = protector.Unprotect(ProtectedValue);
                data = ToValue(unprotectedValue);
                if (object.Equals(RawValue, default(T)))
                    needToSave = false;
                else {
                    RawValue = default(T);
                    needToSave = true;
                }
            }

            return (data, needToSave);
        }
    }

    public class ProtectableString: ProtectableData<string>
    {
        protected override byte[] ToBytes(string value) {
            if (value == null)
                return null;
            return Encoding.Unicode.GetBytes(value);
        }

        protected override string ToValue(byte[] bytes) {
            if (bytes == null)
                return null;
            return Encoding.Unicode.GetString(bytes);
        }
    }

    public class ProtectableBytes: ProtectableData<byte[]>
    {
        protected override byte[] ToBytes(byte[] value) {
            return value;
        }

        protected override byte[] ToValue(byte[] bytes) {
            return bytes;
        }
    }
}
