using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Application.Common.Utility;
using TaskPilot.Application.Services.Implementation;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;
using TaskPilot.Infrastructure.Data;
using TaskPilot.Infrastructure.Repository;

namespace TaskPilot.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Configure Database
            builder.Services.AddDbContext<TaskContext>(option =>
            option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Dependency Injection
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IEmailSender, EmailSender>();
            builder.Services.AddScoped<ISmsSender, SmsSender>();
            builder.Services.AddScoped<IAuthorizationHandler, CustomAuthorizeHandler>();

            // Configuring Identity Framework 
            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<TaskContext>()
                .AddDefaultTokenProviders();

            builder.Services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
            });

            builder.Services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(1);

                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Error/AccessDenied";
                options.SlidingExpiration = true;
            });

            builder.Services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.FromMinutes(0));

            // Configuring Custom Auth
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("CustomPolicy", policy =>
                {
                    policy.Requirements.Add(new CustomRequirement("", ""));
                });
            });

            // WebOptimizer Configuration
            builder.Services.AddWebOptimizer(pipeline => 
            {
                pipeline.AddCssBundle("css/bundle.css", "css/style.min.css", "css/site.css", "lib/bootstrap/bootstrap.css", "lib/bootstrap-icons/font/bootstrap-icons.css");
                pipeline.AddJavaScriptBundle("/js/bundle.js", "lib/jquery/dist/jquery.min.js", "js/*.js", "lib/bootstrap/dist/js/bootstrap.bundle.min.js", "lib/jquery-validation/dist/jquery.validate.js", "lib/jquery-validation-unobtrusive/jquery.validate.unobtrusive.js");
                pipeline.AddCssBundle("css/main_bundle.css", "/lib/bootstrap/dist/css/bootstrap.css", "lib/bootstrap-icons/font/bootstrap-icons.css", "css/site.css", "css/style.min.css", "lib/datatables/css/dataTables.bootstrap4.css", "lib/datatables/css/dataTables.jqueryui.css", "lib/datatables/css/buttons.dataTables.css", "lib/datatables/css/responsive.bootstrap4.css", "lib/datatables/css/select.bootstrap.css", "lib/sweetalert2/sweetalert2.css");
                pipeline.AddJavaScriptBundle("js/main_bundle.js", "lib/jquery/dist/jquery.min.js", "lib/bootstrap/dist/js/bootstrap.bundle.min.js", "js/*.js", "lib/datatables/js/jquery.dataTables.js", "lib/moment.js/moment.js", "lib/datatables/js/jquery.dataTables.js", "lib/datatables/js/dataTables.bootstrap.js", "lib/jszip/jszip.js", "lib/pdfmake/vfs_fonts.js", "lib/datatables/js/dataTables.bootstrap4.js", "lib/datatables/js/dataTables.select.js", "lib/datatables/js/dataTables.responsive.js", "lib/datatables/js/responsive.bootstrap4.js", "lib/datatables/js/dataTables.bootstrap4.js", "lib/jquery-validation/dist/jquery.validate.js", "lib/jquery-validation-unobtrusive/jquery.validate.unobtrusive.js", "lib/jquery-validation/dist/additional-methods.js", "lib/sweetalert2/sweetalert2.js");
                pipeline.MinifyCssFiles();
                pipeline.MinifyJsFiles();
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Configure Custom Error Page (Using Error Code)
            app.UseStatusCodePagesWithRedirects("/Error/{0}");
            app.UseHttpsRedirection();

            app.UseWebOptimizer();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Dashboard}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
