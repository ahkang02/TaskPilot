﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TaskManagementApp.App_Start;
using TaskManagementApp.DAL;
using TaskManagementApp.Models;
using TaskManagementApp.ViewModels;

namespace TaskManagementApp.Controllers
{
    [CustomAuthorize]

    public class PriorityController : Controller
    {
        private readonly TaskContext _context;
        private readonly PrioritiesRepository _prioritiesRepository;
        private readonly PermissionRepository _permissionRepository;
        private readonly TaskRepository _taskRepository;

        private readonly RoleStore<Roles> _roleStore;
        private readonly UserStore<ApplicationUser> _userStore;
        private readonly UserManager<ApplicationUser> _userManager;

        public PriorityController()
        {
            _context = TaskContext.Create();
            _permissionRepository = new PermissionRepository(_context);
            _prioritiesRepository = new PrioritiesRepository(_context);
            _taskRepository = new TaskRepository(_context);

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
                    Priorities priorityToEdit = _prioritiesRepository.GetById(viewModel.Id);
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

        public ActionResult Delete(string[] name)
        {
            if (name.Length > 0)
            {
                for (int i = 0; i < name.Length; i++)
                {
                    var desc = name[i];
                    var priorityToDelete = _prioritiesRepository.GetByName(desc);

                    if (priorityToDelete != null)
                    {
                        if(_taskRepository.GetAll().Any(p => p.PriorityId == priorityToDelete.Id))
                        {
                            TempData["ErrorMsg"] = "Oops something went wrong, you can't delete a priority that being by some task currently.";
                            return RedirectToAction("Index", "Priority");
                        }else
                        {
                            _prioritiesRepository.Delete(priorityToDelete);
                        }
                    }

                }
                _prioritiesRepository.Save();
            }
            _prioritiesRepository.Dispose();
            TempData["SuccessMsg"] = name.Length + " priorites has been deleted successfully";
            return RedirectToAction("Index", "Priority");
        }
    }
}