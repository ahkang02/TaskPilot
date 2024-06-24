using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Domain.Entities;

namespace TaskPilot.Web
{
    public class CustomAuthorizeHandler : AuthorizationHandler<CustomRequirement>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomAuthorizeHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CustomRequirement requirement)
        {
            if (!context.User.Identity!.IsAuthenticated)
            {
                context.Fail();
                return;
            }

            var httpContext = context.Resource as HttpContext;
            var routeData = httpContext!.GetRouteData();

            var controller = routeData.Values["controller"] as string;
            var action = routeData.Values["action"] as string;

            var currentUser = _unitOfWork.Users.GetAllInclude(u => u.Id == context.User.FindFirst(ClaimTypes.NameIdentifier)!.Value, "UserRoles").First();
            var userRoles = _unitOfWork.Roles.Get(r => r.Id == currentUser.UserRoles!.First().RoleId);
            var permissions = _unitOfWork.Permissions.GetAllInclude(null, "Roles,Features");
            var permissionInRole = new List<Permission>();

            foreach (var permission in permissions)
            {
                foreach (var permissionRole in permission.Roles)
                {
                    if (permissionRole.Id == userRoles.Id)
                    {
                        permissionInRole.Add(permission);
                    }
                }
            }

            foreach (var userPermission in permissionInRole)
            {
                if (userPermission.Features.Name == controller)
                {
                    if (userPermission.Name == action)
                    {
                        context.Succeed(requirement);
                    }
                }
            }

        }

    }
}
