
namespace KdSoft.Services.Security
{
    public static class ClaimTypes
    {
        //public const string UserKey = "urn:x-qline:UserKey"; -- use ClaimTypes.NameIdentifier instead
        //public const string UserName = "urn:x-qline:UserName"; -- use ClaimTypes.Name instead
        public const string Permission = "urn:x-qline:Permission";
        public const string ClaimsId = "urn:x-qline:ClaimsId";
        public const string AuthType = "urn:x-qline:AuthType";
    }

    public static class ClaimValueTypes
    {
        public const string AdUserName = "urn:x-qline:AdUserName";
        public const string Guid = "urn:x-qline:Guid";
        public const string TokenSig = "urn:x-qline:TokenSig";
    }

    public static class AuthenticationSchemes
    {
        public const string QLine = "X-QLine";
        public const string OpenId = "X-OpenId";
    }
}
