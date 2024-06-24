using System.Security.Claims;
using TaskPilot.Domain.Entities;

namespace TaskPilot.Application.Services.Interface
{
    public interface IUserPermissionService
    {
        IEnumerable<Permission> GetUserPermission(ClaimsIdentity userClaims);
    }
}
