using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TaskPilot.Domain.Entities;

namespace TaskPilot.Web.Middleware
{
    public class SecurityStampValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityStampValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                var signInManager = context.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();

                var user = await userManager.GetUserAsync(context.User);
                var currentSecurityStamp = context.User.FindFirstValue("AspNet.Identity.SecurityStamp");

                if (user != null && currentSecurityStamp != user.SecurityStamp)
                {
                    context.Response.Cookies.Append("securityStampChanged", "true", new CookieOptions
                    {
                        HttpOnly = false, // Allow client-side scripts to access the cookie
                        Expires = DateTimeOffset.UtcNow.AddMinutes(1)
                    });
                    await signInManager.SignOutAsync();
                }
            }

            await _next(context);
        }
    }
}