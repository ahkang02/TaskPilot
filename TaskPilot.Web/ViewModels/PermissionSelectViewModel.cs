namespace TaskPilot.Web.ViewModels
{
    public class PermissionSelectViewModel
    {
        public Guid PermissionId { get; set; }

        public required string Name { get; set; }

        public bool IsSelected { get; set; }

        public required string FeaturesName { get; set; }
    }
}
