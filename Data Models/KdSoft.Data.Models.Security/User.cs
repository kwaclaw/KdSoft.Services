
namespace KdSoft.Data.Models.Security
{
    public class User
    {
        public int UserKey { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
    }
}
