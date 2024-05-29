namespace TaskPilot.Web.ViewModels
{
    public class FeaturePermissionViewModel
    {
        public string? FeatureName { get; set; }

        public required List<PermissionSelectViewModel> Permissions { get; set; }
    }
}
