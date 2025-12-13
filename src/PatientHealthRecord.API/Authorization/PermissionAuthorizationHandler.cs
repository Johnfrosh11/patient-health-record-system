using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace PatientHealthRecord.API.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var permissions = context.User.Claims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToList();

        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public static class AuthorizationPolicyExtensions
{
    public static void AddPermissionPolicies(this AuthorizationOptions options)
    {
        var permissions = new[]
        {
            Domain.Constants.Permissions.ViewPatientRecords,
            Domain.Constants.Permissions.CreatePatientRecords,
            Domain.Constants.Permissions.UpdatePatientRecords,
            Domain.Constants.Permissions.DeletePatientRecords,
            Domain.Constants.Permissions.ApproveAccessRequests,
            Domain.Constants.Permissions.ManageUsers,
            Domain.Constants.Permissions.ManageRoles
        };

        foreach (var permission in permissions)
        {
            options.AddPolicy(permission, policy =>
                policy.Requirements.Add(new PermissionRequirement(permission)));
        }
    }
}
