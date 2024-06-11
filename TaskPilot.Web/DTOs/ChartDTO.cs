namespace TaskPilot.Web.DTOs
{
    public class ChartDTO
    {
        public int resolvedTasksCount { get; set; }

        public int createdTasksCount { get; set; }

        public int dueTasksCount { get; set; }

        public string Date { get; set; }
    }
}
