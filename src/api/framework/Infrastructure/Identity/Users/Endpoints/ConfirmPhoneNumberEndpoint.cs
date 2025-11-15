using FSH.Framework.Core.Identity.Users.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FSH.Framework.Infrastructure.Identity.Users.Endpoints;

public static class ConfirmPhoneNumberEndpoint
{
    internal static RouteHandlerBuilder MapConfirmPhoneNumberEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/confirm-phone-number", async (
            [FromQuery] string userId,
            [FromQuery] string code,
            IUserService userService) =>
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code))
            {
                return Results.BadRequest("User Id and Code are required.");
            }

            var result = await userService.ConfirmPhoneNumberAsync(userId, code);
            return Results.Ok(result);
        })
        .WithName(nameof(ConfirmPhoneNumberEndpoint))
        .WithSummary("Confirm phone number")
        .WithDescription("Confirms user's phone number using the confirmation token.")
        .AllowAnonymous();
    }
}
