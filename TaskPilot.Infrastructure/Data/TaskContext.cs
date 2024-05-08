using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TaskPilot.Domain.Entities;

namespace TaskPilot.Infrastructure.Data
{
    public class TaskContext : IdentityDbContext<ApplicationUser, Roles, string>
    {
        public TaskContext(DbContextOptions<TaskContext> options) : base(options) { }

        public DbSet<Tasks> Tasks { get; set; }

        public DbSet<Priorities> Priorities { get; set; }

        public DbSet<Statuses> Statuses { get; set; }

        public DbSet<Notifications> Notifications { get; set; }

        public DbSet<Permission> Permissions { get; set; }

        public DbSet<Features> Features { get; set; }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public DbSet<Roles> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
