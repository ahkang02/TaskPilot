using Microsoft.AspNetCore.Mvc.Filters;
using TaskPilot.Application.Common.Utility;

namespace TaskPilot.Web
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class PreventDuplicateRequestAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Request.HasFormContentType && context.HttpContext.Request.Form.ContainsKey("__RequestVerificationToken"))
            {
                var currentToken = context.HttpContext.Request.Form["__RequestVerificationToken"].ToString();
                var lastToken = context.HttpContext.Session.GetString("LastProcessedToken");

                if (lastToken == currentToken)
                {
                    context.ModelState.AddModelError("", Message.DUPLICATE_REQUEST);
                }
                else
                {
                    context.HttpContext.Session.SetString("LastProcessedToken", currentToken);
                }

            }
        }
    }
}
