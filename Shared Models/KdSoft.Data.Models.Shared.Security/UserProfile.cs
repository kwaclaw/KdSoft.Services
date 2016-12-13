using System.Collections.Generic;

namespace KdSoft.Data.Models.Shared.Security
{
    public class UserProfile
    {
        public int UserRef { get; set; }
        public string UserName { get; set; }
        public string AdUserName { get; set; }

        public string Email { get; set; }
        public string Surname { get; set; }
        public string GivenName { get; set; }
        public bool Inactive { get; set; }
        public string JobPosition { get; set; }
        public string CellPhone { get; set; }
        public string HomePhone { get; set; }

        public string Version { get; set; }

        public IEnumerable<string> Locations { get; set; }
        public IEnumerable<string> Roles { get; set; }

        public Dictionary<string, string> ExtraInfo { get; set; }
    }
}