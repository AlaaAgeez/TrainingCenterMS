using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TrainingCenter.Business.Authorization
{
    public class InstructorOwnerOrAdminRequirement : IAuthorizationRequirement { }

    public class InstructorOwnerOrAdminHandler : AuthorizationHandler<InstructorOwnerOrAdminRequirement, int>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            InstructorOwnerOrAdminRequirement requirement,
            int resourceInstructorId) 
        {
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userId, out int authenticatedUserId) && authenticatedUserId == resourceInstructorId)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}