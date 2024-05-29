namespace TaskPilot.Web.DTOs
{
    public class RoleDTO
    {
        public required string RoleId { get; set; }

        public required string RoleName { get; set; }

        public bool IsActive { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        public int Permissions { get; set; }

        public int UserInRole { get; set; }
    }
}
