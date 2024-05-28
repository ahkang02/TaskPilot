using System.ComponentModel.DataAnnotations;

namespace TaskPilot.Web.ViewModels
{
    public class ImportTaskViewModel
    {
        [RegularExpression(@"([a-zA-Z0-9\s_\\.\-:])+(.csv)$", ErrorMessage = "Only csv files allowed.")]
        public IFormFile File { get; set; }

        public List<TaskImportInfo>? ImportInfo { get; set; }
    }
}
