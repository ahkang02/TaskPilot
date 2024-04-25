using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TaskManagementApp.ViewModels
{
    public class ImportTaskViewModel
    {
        [RegularExpression(@"([a-zA-Z0-9\s_\\.\-:])+(.csv)$", ErrorMessage = "Only csv files allowed.")]
        public HttpPostedFile File { get; set; }

        public List<TaskImportInfo> ImportInfo { get; set; }

    }
}