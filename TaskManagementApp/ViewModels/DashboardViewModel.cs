using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TaskManagementApp.Models;

namespace TaskManagementApp.ViewModels
{
    public class DashboardViewModel
    {
        public List<TaskDetailViewModel> UserTaskList { get; set; }

        public Tasks OverDueTask {  get; set; }

        public int dayLeftDue { get; set; }
    }
}