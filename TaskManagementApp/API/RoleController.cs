using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TaskManagementApp.DAL;
using TaskManagementApp.DTO;
using TaskManagementApp.Models;

namespace TaskManagementApp.API
{
    public class RoleController : ApiController
    {
        private readonly TaskContext _context;
        private PermissionRepository _permissionRepository;
        private RoleStore<Roles> _roleStore;
        private RoleManager<Roles> _roleManager;
        private UserStore<ApplicationUser> _userStore;
        private UserManager<ApplicationUser> _userManager;

        public RoleController()
        {
            this._context = TaskContext.Create();
            this._permissionRepository = new PermissionRepository(_context);
            _roleStore = new RoleStore<Roles>(_context);
            _roleManager = new RoleManager<Roles>(_roleStore);
            _userStore = new UserStore<ApplicationUser>(_context);
            _userManager = new UserManager<ApplicationUser>(_userStore);
        }

        public IEnumerable<RoleDTO> GetRoles()
        {
            var roles = _roleStore.Roles.Include(u => u.Users).Include(p => p.Permissions).ToList();

            List<RoleDTO> roleDTOs = new List<RoleDTO>();
            foreach (var role in roles)
            {
                var users = _userStore.Users.Where(u => u.Roles.Any(r => r.RoleId == role.Id)).ToList();
                roleDTOs.Add(new RoleDTO
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
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
