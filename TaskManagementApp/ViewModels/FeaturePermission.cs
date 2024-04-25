using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TaskManagementApp.Models;

namespace TaskManagementApp.ViewModels
{
    public class FeaturePermission
    {
        public string FeatureName { get; set; }
        public List<PermissionSelectViewModel> Permissions { get; set; }
    }
}