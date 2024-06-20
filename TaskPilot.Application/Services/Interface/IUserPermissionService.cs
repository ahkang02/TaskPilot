using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TaskPilot.Domain.Entities;

namespace TaskPilot.Application.Services.Interface
{
    public interface IUserPermissionService
    {
        IEnumerable<Permission> GetUserPermission(ClaimsIdentity userClaims);
    }
}
