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
    public class PriorityController : Controller
    {
        private TaskContext _context;
        private PrioritiesRepository _prioritiesRepository;
        private PermissionRepository _permissionRepository;

        private RoleStore<Roles> _roleStore;
        private UserStore<ApplicationUser> _userStore;
        private UserManager<ApplicationUser> _userManager;

        public PriorityController()
        {
            _context = TaskContext.Create();
            _permissionRepository = new PermissionRepository(_context);
            _prioritiesRepository = new PrioritiesRepository(_context);

            _userStore = new UserStore<ApplicationUser>(_context);
            _userManager = new UserManager<ApplicationUser>(_userStore);

            _roleStore = new RoleStore<Roles>(_context);
        }

        // GET: Priority
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
            EditPriorityViewModel viewModel = new EditPriorityViewModel();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult New(EditPriorityViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (viewModel.Id == null)
                {
                    Priorities priority = new Priorities
                    {
                        Description = viewModel.Name,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                    };

                    _prioritiesRepository.Insert(priority);
                    TempData["SuccessMsg"] = "A new priority indicator has been created";
                }
                else
                {
                    Priorities priorityToEdit = _prioritiesRepository.GetByName(viewModel.Name);
                    priorityToEdit.Description = viewModel.Name;
                    priorityToEdit.UpdatedAt = DateTime.Now;

                    _prioritiesRepository.Update(priorityToEdit);
                    TempData["SuccessMsg"] = priorityToEdit.Description + "'s priority has been updated";
                }
                _prioritiesRepository.Save();
                _prioritiesRepository.Dispose();
                return RedirectToAction("Index", "Priority");
            }
            TempData["ErrorMsg"] = "Oops! Something went wrong, please go through the error message";
            return View(viewModel);
        }

        public ActionResult Update(string name)
        {
            var priorityInDb = _prioritiesRepository.GetByName(name);
            EditPriorityViewModel viewModel = new EditPriorityViewModel
            {
                Id = priorityInDb.Id,
                Name = priorityInDb.Description,
            };
            return View("New", viewModel);
        }
    }
}