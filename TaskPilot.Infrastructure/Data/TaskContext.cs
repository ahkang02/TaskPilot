using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskPilot.Domain.Entities;

namespace TaskPilot.Infrastructure.Data
{
    public class TaskContext : IdentityDbContext<
        ApplicationUser, ApplicationRole, string,
        ApplicationUserClaim, ApplicationUserRole, ApplicationUserLogin,
        ApplicationRoleClaim, ApplicationUserToken>
    {
        public TaskContext(DbContextOptions<TaskContext> options) : base(options) { }

        public TaskContext()
        {

        }
        public DbSet<Tasks> Tasks { get; set; }

        public DbSet<Priorities> Priorities { get; set; }

        public DbSet<Statuses> Statuses { get; set; }

        public DbSet<Notifications> Notifications { get; set; }

        public DbSet<Permission> Permissions { get; set; }

        public DbSet<Features> Features { get; set; }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public DbSet<ApplicationRole> ApplicationRoles { get; set; }

        public DbSet<ApplicationUserRole> UserRoles { get; set; }

        public DbSet<ApplicationRoleClaim> RoleClaims { get; set; }

        public DbSet<ApplicationUserClaim> UserClaims { get; set; }

        public DbSet<ApplicationUserLogin> UserLogins { get; set; }

        public DbSet<ApplicationUserToken> UserTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ApplicationUser>(b =>
            {
                // Each User can have many UserClaims
                b.HasMany(e => e.Claims)
                    .WithOne(e => e.User)
                    .HasForeignKey(uc => uc.UserId)
                    .IsRequired();

                // Each User can have many UserLogins
                b.HasMany(e => e.Logins)
                    .WithOne(e => e.User)
                    .HasForeignKey(ul => ul.UserId)
                    .IsRequired();

                // Each User can have many UserTokens
                b.HasMany(e => e.Tokens)
                    .WithOne(e => e.User)
                    .HasForeignKey(ut => ut.UserId)
                    .IsRequired();

                // Each User can have many entries in the UserRole join table
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.User)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });

            builder.Entity<ApplicationRole>(b =>
            {
                // Each Role can have many entries in the UserRole join table
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.Role)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                // Each Role can have many associated RoleClaims
                b.HasMany(e => e.RoleClaims)
                    .WithOne(e => e.Role)
                    .HasForeignKey(rc => rc.RoleId)
                    .IsRequired();
            });
        }
    }
}
