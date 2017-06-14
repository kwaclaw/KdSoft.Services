using System;
using System.Text;

namespace KdSoft.Services.WebApi
{
    /// <summary>
    /// Encodes and Decodes strings as Base64Url encoding.
    /// </summary>
    public static class Base64UrlEncoder
    {
        static readonly char base64PadCharacter = '=';
        static readonly char base64Character62 = '+';
        static readonly char base64Character63 = '/';
        static readonly char base64UrlCharacter62 = '-';
        static readonly char base64UrlCharacter63 = '_';

        /// <summary>
        /// The following functions perform base64url encoding which differs from regular base64 encoding as follows
        /// * padding is skipped so the pad character '=' doesn't have to be percent encoded
        /// * the 62nd and 63rd regular base64 encoding characters ('+' and '/') are replace with ('-' and '_')
        /// The changes make the encoding alphabet file and URL safe.
        /// </summary>
        /// <param name="arg">string to encode.</param>
        /// <returns>Base64Url encoding of the UTF8 bytes.</returns>
        public static string Encode(string arg) {
            return Encode(Encoding.UTF8.GetBytes(arg));
        }

        /// <summary>
        /// Converts a subset of an array of 8-bit unsigned integers to its equivalent string representation that is encoded with base-64-url digits. Parameters specify
        /// the subset as an offset in the input array, and the number of elements in the array to convert.
        /// </summary>
        /// <param name="inArray">An array of 8-bit unsigned integers.</param>
        /// <param name="length">An offset in inArray.</param>
        /// <param name="offset">The number of elements of inArray to convert.</param>
        /// <returns>The string representation in base 64 url encodingof length elements of inArray, starting at position offset.</returns>
        /// <exception cref="ArgumentNullException">'inArray' is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">offset or length is negative OR offset plus length is greater than the length of inArray.</exception>
        public static string Encode(byte[] inArray, int offset, int length) {
            if (inArray == null) {
                throw new ArgumentNullException("inArray");
            }

            var base64Chars = new char[length * 2];
            int count = Convert.ToBase64CharArray(inArray, offset, length, base64Chars, 0);
            while (base64Chars[--count] == base64PadCharacter) ;
            count++;  // fix up count

            for (int indx = 0; indx < count; indx++) {
                var chr = base64Chars[indx];
                if (chr == base64Character62)
                    base64Chars[indx] = base64UrlCharacter62;
                else if (chr == base64Character63)
                    base64Chars[indx] = base64UrlCharacter63;
            }

            return new string(base64Chars, 0, count);
        }

        /// <summary>
        /// Converts a subset of an array of 8-bit unsigned integers to its equivalent string representation that is encoded with base-64-url digits. Parameters specify
        /// the subset as an offset in the input array, and the number of elements in the array to convert.
        /// </summary>
        /// <param name="inArray">An array of 8-bit unsigned integers.</param>
        /// <returns>The string representation in base 64 url encodingof length elements of inArray, starting at position offset.</returns>
        /// <exception cref="ArgumentNullException">'inArray' is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">offset or length is negative OR offset plus length is greater than the length of inArray.</exception>
        public static string Encode(byte[] inArray) {
            return Encode(inArray, 0, inArray.Length);
        }

        /// <summary>
        ///  Converts the specified string, which encodes binary data as base-64-url digits, to an equivalent 8-bit unsigned integer array.</summary>
        /// <param name="str">base64Url encoded string.</param>
        /// <returns>UTF8 bytes.</returns>
        public static byte[] DecodeBytes(string str) {
            if (str == null) {
                throw new ArgumentNullException("str");
            }

            char[] base64Chars;
            int newLen;

            // check for padding
            switch (str.Length % 4) {
                case 0:
                    // No pad chars in this case
                    base64Chars = new char[str.Length];
                    break;
                case 2:
                    // Two pad chars
                    newLen = str.Length + 2;
                    base64Chars = new char[newLen];
                    base64Chars[--newLen] = base64PadCharacter;
                    base64Chars[--newLen] = base64PadCharacter;
                    break;
                case 3:
                    // One pad char
                    newLen = str.Length + 1;
                    base64Chars = new char[newLen];
                    base64Chars[--newLen] = base64PadCharacter;
                    break;
                default:
                    throw new FormatException("Illegal base64url string!");
            }

            // copy characters while replacing them with proper Base64 characters
            for (int indx = 0; indx < str.Length; indx++) {
                var chr = str[indx];
                if (chr == base64UrlCharacter62)
                    base64Chars[indx] = base64Character62;
                else if (chr == base64UrlCharacter63)
                    base64Chars[indx] = base64Character63;
                else
                    base64Chars[indx] = chr;
            }

            return Convert.FromBase64CharArray(base64Chars, 0, base64Chars.Length);
        }

        /// <summary>
        /// Decodes the string from Base64UrlEncoded to UTF8.
        /// </summary>
        /// <param name="arg">string to decode.</param>
        /// <returns>UTF8 string.</returns>
        public static string Decode(string arg) {
            return Encoding.UTF8.GetString(DecodeBytes(arg));
        }
    }
}