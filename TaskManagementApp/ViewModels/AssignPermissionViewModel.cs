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
        [Required]
        [Display(Name = "Roles")]
        public string RoleId;

        public List<Roles> RoleList;

        [Required]
        public List<FeaturePermission> FeaturePermissions { get; set; }
    }
}