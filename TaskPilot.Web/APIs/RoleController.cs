using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Infrastructure.Repository;
using TaskPilot.Web.DTOs;

namespace TaskPilot.Web.APIs
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public RoleController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<RoleDTO> GetRoles()
        {
            var roles = _unitOfWork.Roles.GetAllInclude(null, "Permissions,UserRoles");

            List<RoleDTO> roleDTOs = new List<RoleDTO>();
            foreach (var role in roles)
            {
                var users = _unitOfWork.Users.GetAllInclude(u => u.UserRoles!.Any(r => r.RoleId == role.Id)).ToList();
                roleDTOs.Add(new RoleDTO
                {
                    RoleId = role.Id,
                    RoleName = role.Name!,
                    IsActive = role.IsActive,
                    Created = role.CreatedAt,
                    Permissions = role.Permissions.Count(),
                    Updated = role.UpdatedAt,
                    UserInRole = users.Count()
                });
            }
            return roleDTOs;
        }
    }
}
