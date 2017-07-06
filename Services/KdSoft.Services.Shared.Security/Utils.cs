using System;

namespace KdSoft.Services.Security
{
    public static class Utils
    {
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
