using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Web.DTOs;

namespace TaskPilot.Web.APIs
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public PermissionController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;    
        }

        public IEnumerable<PermissionDTO> GetPermissions()
        {
            var permissions = _unitOfWork.Permissions.GetAll();
            List<PermissionDTO> permissionDTOs = new List<PermissionDTO>();

            foreach (var permission in permissions)
            {
                permissionDTOs.Add(new PermissionDTO
                {
                    Id = permission.Id,
                    Name = permission.Name,
                    Created = permission.CreatedAt,
                    Updated = permission.UpdatedAt,
                });
            }
            return permissionDTOs;
        }

    }
}
