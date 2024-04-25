using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TaskManagementApp.Models;

namespace TaskManagementApp.ViewModels
{
    public class EditPermissionViewModel
    {
        public Guid? Id { get; set; }

        [Required]
        public string Name { get; set; }

        public Guid FeatureId { get; set; }

        public List<Features> Features { get; set; }
    }
}