using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;

namespace TaskPilot.Application.Services.Implementation
{
    public class PermissionService : IPermissionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PermissionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void CreatePermission(Permission permission)
        {
            _unitOfWork.Permissions.Add(permission);
            _unitOfWork.Save();
        }

        public IEnumerable<Permission> GetAllInclude(string includeProperties)
        {
            return _unitOfWork.Permissions.GetAllInclude(null, includeProperties);
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            return _unitOfWork.Permissions.GetAll();
        }

        public Permission GetPermissionByName(string name)
        {
            return _unitOfWork.Permissions.Get(p => p.Name == name);
        }

        public void UpdatePermission(Permission permission)
        {
            _unitOfWork.Permissions.Update(permission);
            _unitOfWork.Save();
        }

        public IEnumerable<Permission> GetAllPermissionIncludeFeaturesSortByFeaturesName()
        {
            return _unitOfWork.Permissions.GetAllInclude(null, "Features").OrderBy(p => p.Features.Name);
        }

        public Permission GetPermissionById(Guid Id)
        {
            return _unitOfWork.Permissions.Get(p => p.Id == Id);
        }

        // ...

        public IEnumerable<Permission> GetPermissionInRole(ApplicationRole role)
        {
            var permissions = _unitOfWork.Permissions.GetAllInclude(null, "Roles,Features");
            return permissions.AsEnumerable().Where(p => p.Roles.Contains(role));
        }

        public IEnumerable<Permission> GetAllFeaturePermission()
        {
            return _unitOfWork.Permissions.GetAllInclude(filter: null, includeProperties: "Features,Roles");
        }
    }
}
