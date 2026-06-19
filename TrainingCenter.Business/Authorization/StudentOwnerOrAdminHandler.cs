using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TrainingCenter.Business.Authorization
{
    public class OwnerOrAdminRequirement : IAuthorizationRequirement { }

    public class OwnerOrAdminHandler : AuthorizationHandler<OwnerOrAdminRequirement, int>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            OwnerOrAdminRequirement requirement,
            int resourceId)
        {
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userId, out int authenticatedUserId) && authenticatedUserId == resourceId)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}