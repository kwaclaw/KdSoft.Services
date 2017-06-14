using System;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace KdSoft.Services.WebApi
{
    public class FileTicketIssuer
    {
        static readonly char[] tokenSplitChars = new char[] {'.'};

        byte[] symmetricKey;
        TimeSpan lifeTime;

        public FileTicketIssuer(byte[] symmetricKey, TimeSpan lifeTime) {
            this.symmetricKey = symmetricKey;
            this.lifeTime = lifeTime;
        }

        public string CreateFileAccessTicket(Guid fileId, bool forWrite, DateTimeOffset expiryTime) {
            var fileIdBytes = fileId.ToByteArray();
            var expBytes = BitConverter.GetBytes(expiryTime.UtcTicks);
            byte[] hash;

            using (var hmac = new HMACSHA256(symmetricKey)) {
#if COREFX
                var oldLen = fileIdBytes.Length;
                Array.Resize<byte>(ref fileIdBytes, oldLen + expBytes.Length);
                Array.Copy(expBytes, 0, fileIdBytes, oldLen, expBytes.Length);
                hash = hmac.ComputeHash(fileIdBytes);
#else
                hmac.TransformBlock(fileIdBytes, 0, fileIdBytes.Length, null, 0);
                hmac.TransformFinalBlock(expBytes, 0, expBytes.Length);
                hash = hmac.Hash;
#endif
            }

            var sb = new StringBuilder(Base64UrlEncoder.Encode(hash));
            sb.Append('.');
            sb.Append(Base64UrlEncoder.Encode(fileIdBytes));
            if (forWrite)
                sb.Append(".W.");
            else
                sb.Append(".R.");
            sb.Append(Base64UrlEncoder.Encode(expBytes));

            return sb.ToString();
        }

        public string CreateFileAccessTicket(Guid fileId, bool forWrite) {
            var expiryTime = DateTimeOffset.UtcNow + lifeTime;
            return CreateFileAccessTicket(fileId, forWrite, expiryTime);
        }

        public Guid ValidateFileAccessTicket(string ticket, bool forUpdate) {
            var parts = ticket.Split(tokenSplitChars, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 4)
                throw new ArgumentException("Invalid file access ticket format.");

            bool forWrite = parts[2] == "W";
            if (forUpdate && !forWrite)
                throw new SecurityException("File access ticket does not grant write permissions.");

            var decodedHash = Base64UrlEncoder.DecodeBytes(parts[0]);
            var fileIdBytes = Base64UrlEncoder.DecodeBytes(parts[1]);
            var expBytes = Base64UrlEncoder.DecodeBytes(parts[3]);
            byte[] hash;

            using (var hmac = new HMACSHA256(symmetricKey)) {
#if COREFX
                var oldLen = fileIdBytes.Length;
                Array.Resize<byte>(ref fileIdBytes, oldLen + expBytes.Length);
                Array.Copy(expBytes, 0, fileIdBytes, oldLen, expBytes.Length);
                hash = hmac.ComputeHash(fileIdBytes);
#else
                hmac.TransformBlock(fileIdBytes, 0, fileIdBytes.Length, null, 0);
                hmac.TransformFinalBlock(expBytes, 0, expBytes.Length);
                hash = hmac.Hash;
#endif
                if (!KdSoft.Utils.Common.Equals<byte>(decodedHash, hash))
                    throw new SecurityException("Invalid signature for file access token.");
            }

            var expiryTicks = BitConverter.ToInt64(expBytes, 0);
            if (expiryTicks < DateTimeOffset.UtcNow.UtcTicks)
                    throw new ArgumentException("File access token is expired.");

            return new Guid(fileIdBytes);
        }
    }
}
