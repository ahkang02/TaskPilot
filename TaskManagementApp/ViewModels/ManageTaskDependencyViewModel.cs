using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TaskManagementApp.Models;

namespace TaskManagementApp.ViewModels
{
    public class ManageTaskDependencyViewModel
    {
        [Display(Name = "Current Task")]
        public string CurrentTask {  get; set; }

        public List<Tasks> ListOfTasks { get; set; }

        [Display(Name = "Assign Dependency")]
        public Guid? DependencyID { get; set; }
    }
}