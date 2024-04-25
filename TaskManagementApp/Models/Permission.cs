﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TaskManagementApp.Models
{
    public class Permission
    {
        public Permission()
        {
            this.Roles = new HashSet<Roles>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
        
        public virtual ICollection<Roles> Roles { get; set; }

        public Guid featuresId { get; set; }
        public Features features {  get; set; }
    }
}