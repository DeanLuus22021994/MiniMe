using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace FSH.Framework.Infrastructure.Storage.Endpoints;

internal static class Extensions
{
    public static IEndpointRouteBuilder MapStorageEndpoints(this IEndpointRouteBuilder app)
    {
        var storageGroup = app.MapGroup("api/storage");
        
        storageGroup.MapUploadFileEndpoint();
        
        return app;
    }
}
