﻿using TaskPilot.Domain.Entities;

namespace TaskPilot.Application.Services.Interface
{
    public interface IPermissionService
    {
        IEnumerable<Permission> GetAllPermissions();

        IEnumerable<Permission> GetAllInclude(string includeProperties);

        IEnumerable<Permission> GetAllFeaturePermission();

        Permission GetPermissionByName(string name);

        Permission GetPermissionById(Guid Id);

        void CreatePermission(Permission permission);

        void UpdatePermission(Permission permission);

        IEnumerable<Permission> GetAllPermissionIncludeFeaturesSortByFeaturesName();

        IEnumerable<Permission> GetPermissionInRole(ApplicationRole role);
    }
}
