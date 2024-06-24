using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;
using TaskPilot.Web.ViewModels;

namespace TaskPilot.Web.ViewComponents
{
    public class SidebarViewComponent : ViewComponent
    {
        private readonly IUserPermissionService _userPermissionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SidebarViewComponent(UserManager<ApplicationUser> userManager, IUserPermissionService userPermissionService)
        {
            _userManager = userManager;
            _userPermissionService = userPermissionService;
        }

        public IViewComponentResult Invoke()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity!;
            UserPermissionViewModel viewModel = new UserPermissionViewModel
            {
                UserPermissions = _userPermissionService.GetUserPermission(claimsIdentity).ToList()
            };
            return View(viewModel);
        }

    }
}
