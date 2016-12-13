
namespace KdSoft.Data.Models.Security
{
    public class Role
    {
        public string RoleKey { get; set; }
        public string Name { get; set; }
    }

    public class RoleInfo: Role
    {
        public string Description { get; set; }
    }
}
