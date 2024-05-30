using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Domain.Entities;
using TaskPilot.Web.ViewModels;

namespace TaskPilot.Web
{
    public static class Helper
    {
        public static UserPermissionViewModel GetUserPermission(IUnitOfWork unitOfWork, ClaimsIdentity userClaims)
        {
            if (userClaims == null)
                throw new ArgumentNullException(nameof(userClaims));

            UserPermissionViewModel viewModel = new UserPermissionViewModel()
            {
                UserPermissions = new List<Permission>()
            };

            var userId = userClaims.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var userRole = userClaims.FindFirst(ClaimTypes.Role)!.Value;
            var currentUser = unitOfWork.Users.Get(u => u.Id == userId);
            var currentUserRole = unitOfWork.Roles.GetAllInclude(r => r.Name == userRole, "Permissions").First();
            var permissions = unitOfWork.Permissions.GetAllInclude(null, "Features,Roles");

            foreach(var permission in permissions)
            {
                if(permission.Roles.Any(r => r.Id == currentUserRole.Id))
                {
                    viewModel.UserPermissions.Add(permission);
                }
            }

            return viewModel;
        }
    }
}
