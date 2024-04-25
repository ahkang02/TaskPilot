using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TaskManagementApp.Models
{
    public class Notifications
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Description { get; set; }

        public string Status { get; set; }

        public Guid? TasksId { get; set; }

        public Tasks Tasks { get; set; }

        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}