using FSH.Framework.Core.Identity.Users.Abstractions;
using FSH.Starter.Shared.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FSH.Framework.Infrastructure.Identity.Users.Endpoints;

public static class ConfirmEmailEndpoint
{
    internal static RouteHandlerBuilder MapConfirmEmailEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/confirm-email", async (
            [FromQuery] string? userId,
            [FromQuery] string? code,
            [FromHeader(Name = TenantConstants.Identifier)] string tenant,
            IUserService userService,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.BadRequest("User Id is required.");
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                return Results.BadRequest("Confirmation code is required.");
            }

            if (string.IsNullOrWhiteSpace(tenant))
            {
                return Results.BadRequest("Tenant is required.");
            }

            var result = await userService.ConfirmEmailAsync(userId, code, tenant, cancellationToken);
            return Results.Ok(result);
        })
        .WithName(nameof(ConfirmEmailEndpoint))
        .WithSummary("Confirm email address")
        .WithDescription("Confirms user's email address using the confirmation token sent via email.")
        .AllowAnonymous();
    }
}
