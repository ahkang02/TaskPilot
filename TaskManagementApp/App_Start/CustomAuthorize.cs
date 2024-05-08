using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using System.Web.Routing;
using TaskManagementApp.DAL;
using TaskManagementApp.Models;

namespace TaskManagementApp.App_Start
{
    public class CustomAuthorize : AuthorizeAttribute
    {
        private TaskContext _context;
        private UserStore<ApplicationUser> _userStore;
        private UserManager<ApplicationUser> _userManager;
        private RoleStore<Roles> _roleStore;
        private RoleManager<Roles> _roleManager;
        private PermissionRepository _permissionRepository;

        public CustomAuthorize()
        {
            _context = TaskContext.Create();
            _userStore = new UserStore<ApplicationUser>(_context);
            _userManager = new UserManager<ApplicationUser>(_userStore);
            _roleStore = new RoleStore<Roles>(_context);
            _roleManager = new RoleManager<Roles>(_roleStore);
            _permissionRepository = new PermissionRepository(_context);
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var userId = httpContext.User.Identity.GetUserId();
            var userRole = GetUserRoles(userId);
            var userPermission = GetUserPermissions(userRole);
            var controller = httpContext.Request.RequestContext.RouteData.Values["controller"] as string;
            var action = httpContext.Request.RequestContext.RouteData.Values["action"] as string;

            if (!httpContext.User.Identity.IsAuthenticated)
                return false;

            foreach(var permission in userPermission)
            {
                if(permission.Features.Name == controller)
                {
                    if(permission.Name == action)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private Roles GetUserRoles(string userId)
        {
            var roleName = _userManager.GetRoles(userId)[0].ToString();
            var role = _roleStore.Roles.SingleOrDefault(r => r.Name == roleName);
            return role;
        }

        private List<Permission> GetUserPermissions(Roles userRole)
        {

            var permissions = _permissionRepository.GetAllInclude(includeProperties: "Roles, Features");
            List<Permission> permissionInRole = new List<Permission>();

            foreach( var permission in permissions)
            {
                foreach(var  role in permission.Roles)
                {
                    if(role.Id == userRole.Id)
                    {
                        permissionInRole.Add(permission);
                    }
                }
            }

            return permissionInRole;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext actionContext)
        {
            if (!actionContext.HttpContext.User.Identity.IsAuthenticated)
            {
                actionContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Account", action = "Login" }));
            }
            var routeValues = new RouteValueDictionary(new
            {
                controller = "Error",
                action = "AccessDenied"
            });

            actionContext.Result = new RedirectToRouteResult(routeValues);
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (this.AuthorizeCore(filterContext.HttpContext))
            {
                base.OnAuthorization(filterContext);
            }
            else
            {
                this.HandleUnauthorizedRequest(filterContext);
            }
        }
    }
}