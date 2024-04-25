using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TaskManagementApp.Models
{
    public class Tasks
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid? Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public Priorities Priority { get; set; }

        public Statuses Status { get; set; }

        public Guid PriorityId { get; set; }

        public Guid StatusId { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime Created { get; set; }

        public ApplicationUser AssignTo { get; set; }

        public string AssignToId { get; set; }

        public ApplicationUser AssignFrom { get; set; }

        public string AssignFromId { get; set; }

        public Nullable<Guid> DependencyId { get; set; }
    }
}