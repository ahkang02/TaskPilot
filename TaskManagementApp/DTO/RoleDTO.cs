using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TaskManagementApp.Models;

namespace TaskManagementApp.DTO
{
    public class RoleDTO
    {
        public string RoleId { get; set; }

        public string RoleName { get; set; }

        public bool IsActive { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        public int Permissions { get; set; }

        public int UserInRole { get; set; }

    }
}