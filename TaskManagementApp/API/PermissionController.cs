using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using TaskManagementApp.DAL;
using TaskManagementApp.DTO;

namespace TaskManagementApp.API
{
    public class PermissionController : ApiController
    {
        private readonly TaskContext _context;
        private PermissionRepository _permissionRepository;

        public PermissionController()
        {
            _context = TaskContext.Create();
            _permissionRepository = new PermissionRepository(_context);
        }

        public IEnumerable<PermissionDTO> GetPermissions()
        {
            var permissions = _permissionRepository.GetAll();
            List<PermissionDTO> permissionDTOs = new List<PermissionDTO>();

            foreach (var permission in permissions)
            {
                permissionDTOs.Add(new PermissionDTO
                {
                    Id = permission.Id,
                    Name = permission.Name,
                    Created = permission.CreatedAt,
                    Updated = permission.UpdatedAt,
                });
            }
            return permissionDTOs;
        }

        [HttpDelete]
        public IHttpActionResult Delete(string Id)
        {
            var permissionInDb = _permissionRepository.GetByName(Id);

            if (permissionInDb != null)
            {
                if(permissionInDb.Roles.Count > 0) {
                    return BadRequest();
                }else
                {
                    _permissionRepository.Delete(permissionInDb);
                }
            }else
            {
                return NotFound();
            }
            _permissionRepository.Save();
            _permissionRepository.Dispose();
            return Ok();
        }

    }
}