using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TaskManagementApp.Models;

namespace TaskManagementApp.ViewModels
{
    public class EditRoleViewModel
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}