using Microsoft.AspNetCore.Authorization;

namespace TaskPilot.Web
{
    public class CustomRequirement : IAuthorizationRequirement
    {
        public string controller { get; }
        public string action { get; }

        public CustomRequirement(string controller, string action)
        {
            this.controller = controller;
            this.action = action;
        }
    }
}
