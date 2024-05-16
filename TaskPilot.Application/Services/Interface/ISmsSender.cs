using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskPilot.Application.Services.Interface
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string to, string body, string text);
    }
}
