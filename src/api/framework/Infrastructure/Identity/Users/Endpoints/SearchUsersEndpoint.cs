using FSH.Framework.Core.Identity.Users.Abstractions;
using FSH.Framework.Core.Identity.Users.Dtos;
using FSH.Framework.Core.Identity.Users.Features.SearchUsers;
using FSH.Framework.Core.Paging;
using FSH.Framework.Infrastructure.Auth.Policy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FSH.Framework.Infrastructure.Identity.Users.Endpoints;

public static class SearchUsersEndpoint
{
    internal static RouteHandlerBuilder MapSearchUsersEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/search", async (
            [FromBody] SearchUsersCommand request,
            IUserService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.SearchAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName(nameof(SearchUsersEndpoint))
        .WithSummary("Search users")
        .WithDescription("Search and filter users with pagination and sorting support")
        .Produces<PagedList<UserDetail>>()
        .RequirePermission("Permissions.Users.View");
    }
}
