using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TaskManagementApp.DAL;
using TaskManagementApp.Models;
using TaskManagementApp.ViewModels;

namespace TaskManagementApp.Controllers
{

    public class UIController : Controller
    {
        private readonly UserStore<ApplicationUser> _userStore;
        private readonly PermissionRepository _permissionRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleStore<Roles> _roleStore;

        public UIController()
        {
            _permissionRepository = new PermissionRepository(new TaskContext());
            _userStore = new UserStore<ApplicationUser>(new TaskContext());
            _userManager = new UserManager<ApplicationUser>(_userStore);
            _roleStore = new RoleStore<Roles>(new TaskContext());
        }

        // GET: UI
        public ActionResult Sidebar()
        {
            var currentUser = User.Identity.GetUserId();
            var currentUserRole = _userManager.GetRoles(currentUser)[0];
            var roles = _roleStore.Roles.Include("Permissions").SingleOrDefault(r => r.Name == currentUserRole);
            var permissions = _permissionRepository.GetAllInclude(includeProperties: "Features, Roles").ToList();
            

            UserPermissionViewModel viewModel = new UserPermissionViewModel
            {
                UserPermissions = new List<Permission>()
            };

            foreach(var permission in permissions)
            {
                if(permission.Roles.Any(r => r.Id == roles.Id))
                {
                    viewModel.UserPermissions.Add(permission);
                }
            }

            return View(viewModel);
        }
    }
}