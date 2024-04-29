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
    public class StatusController : Controller
    {
        private TaskContext _context;
        private StatusesRepository _statusesRepository;
        private PermissionRepository _permissionRepository;

        private RoleStore<Roles> _roleStore;
        private UserStore<ApplicationUser> _userStore;
        private UserManager<ApplicationUser> _userManager;


        public StatusController()
        {
            _context = TaskContext.Create();
            _statusesRepository = new StatusesRepository(_context);
            _permissionRepository = new PermissionRepository(_context);

            _userStore = new UserStore<ApplicationUser>(_context);
            _userManager = new UserManager<ApplicationUser>(_userStore);

            _roleStore = new RoleStore<Roles>(_context);
        }
        // GET: Status
        public ActionResult Index()
        {
            var currentUser = User.Identity.GetUserId();
            var currentUserRole = _userManager.GetRoles(currentUser)[0];
            var roles = _roleStore.Roles.Include("Permissions").SingleOrDefault(r => r.Name == currentUserRole);
            var permissions = _permissionRepository.GetAllInclude(includeProperties: "Features, Roles").ToList();

            UserPermissionViewModel viewModel = new UserPermissionViewModel
            {
                UserPermissions = new List<Permission>()
            };

            foreach (var permission in permissions)
            {
                if (permission.Roles.Any(r => r.Id == roles.Id))
                {
                    viewModel.UserPermissions.Add(permission);
                }
            }

            return View(viewModel);
        }

        public ActionResult New()
        {
            EditStatusViewModel viewModel = new EditStatusViewModel();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult New(EditStatusViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (viewModel.Id == null)
                {
                    Statuses status = new Statuses
                    {
                        Description = viewModel.Name,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                    };

                    _statusesRepository.Insert(status);
                    TempData["SuccessMsg"] = "A new status has been created";
                }
                else
                {
                    Statuses statusToEdit = _statusesRepository.GetByName(viewModel.Name);
                    statusToEdit.Description = viewModel.Name;
                    statusToEdit.UpdatedAt = DateTime.Now;

                    _statusesRepository.Update(statusToEdit);
                    TempData["SuccessMsg"] = statusToEdit.Description + "'s Status has been updated.";
                }
                _statusesRepository.Save();
                _statusesRepository.Dispose();
                return RedirectToAction("Index", "Status");
            }
            TempData["ErrorMsg"] = "Oops! Something went wrong, please go through the error message";
            return View(viewModel);
        }

        public ActionResult Update(string name)
        {
            var statusInDb = _statusesRepository.GetByName(name);
            EditStatusViewModel viewModel = new EditStatusViewModel
            {
                Id = statusInDb.Id,
                Name = statusInDb.Description,
            };

            return View("New", viewModel);
        }

    }
}