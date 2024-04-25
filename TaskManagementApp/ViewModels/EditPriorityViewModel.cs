using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TaskManagementApp.ViewModels
{
    public class EditPriorityViewModel
    {
        public Guid? Id { get; set; }

        [Required]
        public string Name { get; set; }

    }
}