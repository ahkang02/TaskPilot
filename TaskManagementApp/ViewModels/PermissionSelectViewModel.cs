using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaskManagementApp.ViewModels
{
    public class PermissionSelectViewModel
    {
        public Guid PermissionId { get; set; }

        public string Name { get; set; }

        public bool IsSelected { get; set; }

        public string FeaturesName { get; set; }
    }
}