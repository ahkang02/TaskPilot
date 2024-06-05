using System.ComponentModel.DataAnnotations;

namespace TaskPilot.Web.ViewModels
{
    public class ImportTaskViewModel
    {
        [RegularExpression(@"([a-zA-Z0-9\s_\\.\-:])+(.csv)$")]
        public IFormFile? File { get; set; }

        public required List<TaskImportInfo> ImportInfo { get; set; }
    }
}
