using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TaskManagementApp.CustomValidator;

namespace TaskManagementApp.ViewModels
{
    public class TaskImportInfo
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [DateLessThanToday(ErrorMessage = "Due date cannot be in the past")]
        public DateTime DueDate { get; set; }

        [Required]
        public string PriorityLevel { get; set; }

        [Required]
        public string Status { get; set; }

        [Required]
        public string AssignToUser { get; set; }
    }
}