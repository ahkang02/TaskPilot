﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.DataProtection;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.Entity;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TaskManagementApp.DAL;
using TaskManagementApp.Models;
using TaskManagementApp.ViewModels;
using TaskManagementApp.App_Start;
using Microsoft.Ajax.Utilities;
using System.Security;
using System.Text.RegularExpressions;
using System.Text;

namespace TaskManagementApp.Controllers
{
    public class SystemController : Controller
    {
        private TaskContext _context;
        private PermissionRepository _permissionRepository;
        private PrioritiesRepository _prioritiesRepository;
        private StatusesRepository _statusesRepository;
        private FeaturesRepository _featuresRepository;

        private RoleStore<Roles> _roleStore;
        private RoleManager<Roles> _roleManager;
        private UserStore<ApplicationUser> _userStore;
        private UserManager<ApplicationUser> _userManager;

        public SystemController()
        {
            _context = TaskContext.Create();
            _permissionRepository = new PermissionRepository(_context);
            _prioritiesRepository = new PrioritiesRepository(_context);
            _statusesRepository = new StatusesRepository(_context);
            _featuresRepository = new FeaturesRepository(_context);

            _userStore = new UserStore<ApplicationUser>(_context);
            _userManager = new UserManager<ApplicationUser>(_userStore);

            _roleStore = new RoleStore<Roles>(_context);
            _roleManager = new RoleManager<Roles>(_roleStore);
        }

        #region Permission
        public ActionResult PermissionManagement()
        {
            return View();
        }

        public ActionResult NewPermission()
        {
            EditPermissionViewModel viewModel = new EditPermissionViewModel
            {
                Features = _featuresRepository.GetAll().ToList()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NewPermission(EditPermissionViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (viewModel.Id == null)
                {
                    Permission permission = new Permission
                    {
                        Name = viewModel.Name,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        featuresId = viewModel.FeatureId,
                    };
                    TempData["SuccessMsg"] = "A new permission has been created";
                    _permissionRepository.Insert(permission);
                }
                else
                {
                    Permission permissionToEdit = _permissionRepository.GetByName(viewModel.Name);
                    permissionToEdit.Name = viewModel.Name;
                    permissionToEdit.UpdatedAt = DateTime.Now;

                    _permissionRepository.Update(permissionToEdit);
                    TempData["SuccessMsg"] = permissionToEdit.Name + "'s permission has been updated";

                }
                _permissionRepository.Save();
                _permissionRepository.Dispose();
                return RedirectToAction("PermissionManagement", "System");
            }
            TempData["ErrorMsg"] = "Oops! Something went wrong, please go through the error message";
            return View(viewModel);
        }

        public ActionResult EditPermission(string name)
        {
            var permissionInDb = _permissionRepository.GetByName(name);
            EditPermissionViewModel viewModel = new EditPermissionViewModel
            {
                Id = permissionInDb.Id,
                Name = permissionInDb.Name,
            };
            return View("NewPermission", viewModel);
        }

        #endregion
    }
}