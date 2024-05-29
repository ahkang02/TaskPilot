using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Domain.Entities;
using TaskPilot.Web.DTOs;

namespace TaskPilot.Web.APIs
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<IEnumerable<UserDTO>> GetUsers()
        {
            var user = _unitOfWork.Users.GetAll();
            List<UserDTO> userDTOs = new List<UserDTO>();

            foreach (var u in user)
            {
                var rolesForUser = await _userManager.GetRolesAsync(u);
                userDTOs.Add((new UserDTO
                {
                    Id = u.Id,
                    Email = u.Email!,
                    Username = u.UserName!,
                    UserRole = rolesForUser[0],
                    AccessFailedCount = u.AccessFailedCount,
                    Name = u.LastName + " " + u.FirstName,
                    LastLogin = u.LastLogin,
                    Created = u.CreatedAt,
                    LastUpdated = u.UpdatedAt
                }));
            }

            return userDTOs;
        }

    }
}
