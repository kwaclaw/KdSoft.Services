
namespace KdSoft.Services.Security
{
    public static class ClaimTypes
    {
        //public const string UserKey = "urn:x-kdsoft:UserKey"; -- use ClaimTypes.NameIdentifier instead
        //public const string UserName = "urn:x-kdsoft:UserName"; -- use ClaimTypes.Name instead
        public const string Permission = "urn:x-kdsoft:Permission";
        public const string ClaimsId = "urn:x-kdsoft:ClaimsId";
        public const string AuthType = "urn:x-kdsoft:AuthType";
        public const string AdSecurityGroup = "urn:x-kdsoft:AdSecurityGroup";
        public const string AuthTimeUtc = "urn:x-kdsoft:AuthTimeUtc";
    }

    public static class ClaimValueTypes
    {
        public const string AdUserName = "urn:x-kdsoft:AdUserName";
        public const string Guid = "urn:x-kdsoft:Guid";
        public const string TokenSig = "urn:x-kdsoft:TokenSig";
    }

    public static class AuthenticationSchemes
    {
        public const string KdSoft = "X-KdSoft";
        public const string OpenId = "X-OpenId";
    }
}
