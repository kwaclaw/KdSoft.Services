using KdSoft.Serialization.Buffer;
using System;

namespace KdSoft.Services.Security
{
    /// <summary>
    /// Static utility/helper methods.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Serializer like <see cref="System.BitConverter"/>, with methods to serialize into existing byte arrays.
        /// </summary>
        public static readonly ByteConverter Converter = new ByteConverter(BitConverter.IsLittleEndian ? ByteOrder.LittleEndian : ByteOrder.BigEndian);

        /// <summary>
        /// Encodes URL into Base64 string.
        /// </summary>
        /// <param name="arg">UTF-8 encoded URL.</param>
        /// <returns>Base64 encoded URL.</returns>
        public static string Base64UrlEncode(byte[] arg) {
            int len = arg.Length;
            int outCapacity = ((len + 2 - ((len + 2) % 3)) / 3) * 4;
            char[] chars = new char[outCapacity];
            int outLength = Convert.ToBase64CharArray(arg, 0, arg.Length, chars, 0);

            while (chars[outLength - 1] == '=')  // Remove any trailing '='s
                outLength--;

            for (int indx = 0; indx < outLength; indx++) {
                switch (chars[indx]) {
                    case '+': chars[indx] = '-'; break; // 62nd char of encoding
                    case '/': chars[indx] = '_'; break; // 63rd char of encoding
                    default: break;
                }
            }

            return new string(chars, 0, outLength);
        }

        /// <summary>
        /// Decodes Base64 encoded URL.
        /// </summary>
        /// <param name="arg">Base64 encoded URL.</param>
        /// <returns>UTF-8 encoded URL.</returns>
        public static byte[] Base64UrlDecode(string arg) {
            char[] chars = null;
            switch (arg.Length % 4) // Pad with trailing '='s
            {
                case 0:
                    chars = new char[arg.Length];
                    break; // No pad chars in this case
                case 2:
                    chars = new char[arg.Length + 2];
                    chars[chars.Length - 1] = '=';
                    chars[chars.Length - 2] = '=';
                    break; // Two pad chars
                case 3:
                    chars = new char[arg.Length + 1];
                    chars[chars.Length - 1] = '=';
                    break; // One pad char
                default: throw new ArgumentException("Illegal base64url string!");
            }
            arg.CopyTo(0, chars, 0, arg.Length);

            for (int indx = 0; indx < arg.Length; indx++) {
                switch (chars[indx]) {
                    case '-': chars[indx] = '+'; break; // 62nd char of encoding
                    case '_': chars[indx] = '/'; break; // 63rd char of encoding
                    default: break;
                }
            }

            return Convert.FromBase64CharArray(chars, 0, chars.Length); // Standard base64 decoder
        }
    }
}
