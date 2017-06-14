namespace KdSoft.Services.Security
{
    public interface IAuthorizationScope
    {
        AuthorizationClaimsCache ClaimsCache { get; }
        IAuthorizationProvider Provider { get; }
    }
}
