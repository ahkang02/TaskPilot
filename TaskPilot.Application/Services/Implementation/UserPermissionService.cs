using System.Security.Claims;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;

namespace TaskPilot.Application.Services.Implementation
{
    public class UserPermissionService : IUserPermissionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserPermissionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Permission> GetUserPermission(ClaimsIdentity userClaims)
        {
            if (userClaims == null)
                throw new ArgumentNullException(nameof(userClaims));

            List<Permission> userPermissions = new List<Permission>();

            var userRole = userClaims.FindFirst(ClaimTypes.Role)!.Value;
            var currentUserRole = _unitOfWork.Roles.GetAllInclude(r => r.Name == userRole, "Permissions").First();
            var permissions = _unitOfWork.Permissions.GetAllInclude(null, "Features,Roles");
            userPermissions.AddRange(from permission in permissions
                                     where permission.Roles.Any(r => r.Id == currentUserRole.Id)
                                     select permission);
            return userPermissions;
        }
    }
}
