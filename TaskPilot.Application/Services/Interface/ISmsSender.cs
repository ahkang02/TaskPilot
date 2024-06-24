namespace TaskPilot.Application.Services.Interface
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string to, string body, string text);
    }
}
