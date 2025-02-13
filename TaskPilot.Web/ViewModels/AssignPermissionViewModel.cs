﻿using System.ComponentModel.DataAnnotations;

namespace TaskPilot.Web.ViewModels
{
    public class AssignPermissionViewModel
    {
        public string? RoleId { get; set; }

        [Display(Name = "Current Role")]
        public string? RoleName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        public required List<FeaturePermissionViewModel> FeaturePermissions { get; set; }
    }
}
