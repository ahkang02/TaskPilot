using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TaskManagementApp.Models;

namespace TaskManagementApp.ViewModels
{
    public class AssignPermissionViewModel
    {
        public string RoleId { get; set; }

        [Display(Name = "Current Role")]
        public string RoleName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Required]
        public List<FeaturePermission> FeaturePermissions { get; set; }
    }
}