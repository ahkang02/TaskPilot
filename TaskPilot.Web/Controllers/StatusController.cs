using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Abstractions;
using System.Security.Claims;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Application.Common.Utility;
using TaskPilot.Domain.Entities;
using TaskPilot.Web.ViewModels;

namespace TaskPilot.Web.Controllers
{
    [Authorize(Policy = "CustomPolicy")]
    public class StatusController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public StatusController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity!;
            return View(Helper.GetUserPermission(_unitOfWork, claimsIdentity));

        }

        public IActionResult New()
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
                        ColorCode = viewModel.ColorCode!,
                        Description = viewModel.Name!,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                    };

                    _unitOfWork.Status.Add(status);
                    TempData["SuccessMsg"] = Message.STAT_CREATION;
                }
                else
                {
                    Statuses statusToEdit = _unitOfWork.Status.Get(s => s.Id == viewModel.Id);
                    statusToEdit.Description = viewModel.Name!;
                    statusToEdit.UpdatedAt = DateTime.Now;

                    _unitOfWork.Status.Update(statusToEdit);
                    TempData["SuccessMsg"] = statusToEdit.Description + Message.STAT_UPDATE;
                }
                _unitOfWork.Save();
                return RedirectToAction("Index", "Status");
            }
            TempData["ErrorMsg"] = Message.COMMON_ERROR;
            return View(viewModel);
        }

        public IActionResult Update(string name)
        {
            var statusInDb = _unitOfWork.Status.Get(s => s.Description == name);
            EditStatusViewModel viewModel = new EditStatusViewModel
            {
                Id = statusInDb.Id,
                Name = statusInDb.Description,
                ColorCode = statusInDb.ColorCode != null ? statusInDb.ColorCode : "",
            };

            return View("New", viewModel);
        }

        public IActionResult Delete(Guid[] status)
        {
            var statusToDelete = new List<Statuses>();
            if (status.Length > 0)
            {
                for (int i = 0; i < status.Length; i++)
                {
                    var statusId = status[i];
                    statusToDelete.Add(_unitOfWork.Status.Get(s => s.Id == statusId));
                }

                if (statusToDelete != null)
                {
                    foreach (var statuses in statusToDelete)
                    {
                        if (_unitOfWork.Tasks.GetAll().Any(s => s.StatusId == statuses.Id))
                        {
                            TempData["ErrorMsg"] = Message.STAT_DELETION_FAIL;
                            return Json(Url.Action("Index", "Status"));
                        }
                        else
                        {
                            _unitOfWork.Status.Remove(statuses);
                        }
                    }
                }

            }
            _unitOfWork.Save();
            TempData["SuccessMsg"] = status.Length + Message.STAT_DELETION;
            return Json(Url.Action("Index", "Status"));
        }
    }

}
