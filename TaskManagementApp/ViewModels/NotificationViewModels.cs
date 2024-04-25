using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TaskManagementApp.Models;

namespace TaskManagementApp.ViewModels
{
    public class NotificationViewModels
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Status { get; set; }

        public Guid? taskId { get; set; }

        public DateTime CreatedAt { get; set; }

        public ApplicationUser User { get; set; }
    }
}