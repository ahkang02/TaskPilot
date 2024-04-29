using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TaskManagementApp.Models;

namespace TaskManagementApp.DAL
{
    public class TaskContext : IdentityDbContext<ApplicationUser>
    {
        public TaskContext() : base("TaskContext", throwIfV1Schema: false) { }

        public DbSet<Tasks> Tasks { get; set; }

        public DbSet<Priorities> Priorities { get; set; }

        public DbSet<Statuses> Statuses { get; set; }

        public DbSet<Notifications> Notifications { get; set; }

        public DbSet<Permission> Permissions { get; set; }

        public DbSet<Features> Features { get; set; }

        public static TaskContext Create()
        {
            return new TaskContext();
        }

    }
}

