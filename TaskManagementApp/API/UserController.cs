using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TaskManagementApp.DAL;
using TaskManagementApp.DTO;
using TaskManagementApp.Models;

namespace TaskManagementApp.API
{
    public class UserController : ApiController
    {
        private readonly TaskContext _context;
        private UserStore<ApplicationUser> _userStore;
        private UserManager<ApplicationUser> _userManager;
        private RoleStore<Roles> _roleStore;

        public UserController()
        {
            this._context = TaskContext.Create();
            _userStore = new UserStore<ApplicationUser>(_context);
            _userManager = new UserManager<ApplicationUser>(_userStore);
            _roleStore = new RoleStore<Roles>(_context);
        }

        public IEnumerable<UserDTO> GetUser()
        {
            var user = _userStore.Users.ToList();
            var roles = _roleStore.Roles.ToList();
            List<UserDTO> userDTOs = new List<UserDTO>();

            foreach (var u in user)
            {
                var rolesForUser = _userManager.GetRoles(u.Id);
                userDTOs.Add((new UserDTO
                {
                    Id = u.Id,
                    Email = u.Email,
                    Username = u.UserName,
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
