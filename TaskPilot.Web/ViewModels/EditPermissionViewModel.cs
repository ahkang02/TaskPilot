using System.ComponentModel.DataAnnotations;
using TaskPilot.Domain.Entities;

namespace TaskPilot.Web.ViewModels
{
    public class EditPermissionViewModel
    {
        public Guid? Id { get; set; }

        [Required]
        public string? Name { get; set; }

        public Guid FeatureId { get; set; }

        public List<Features>? Features { get; set; }
    }
}
