using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TaskManagementApp.Models;

namespace TaskManagementApp.ViewModels
{
    public class AssignRoleViewModel
    {
        [Display(Name = "New Role")]
        public string RoleId { get; set; }

        [Display(Name = "User")]
        public string Username { get; set; }

        [Display(Name = "Current User Role")]
        public string CurrentUserRole { get; set; }

        public List<Roles> RoleToSelect { get; set; }

    }
}