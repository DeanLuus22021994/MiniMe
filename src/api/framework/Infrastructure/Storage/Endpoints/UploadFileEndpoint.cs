using FluentValidation;
using FluentValidation.Results;
using FSH.Framework.Core.Storage;
using FSH.Framework.Core.Storage.File;
using FSH.Framework.Core.Storage.File.Features;
using FSH.Framework.Infrastructure.Auth.Policy;
using FSH.Starter.Shared.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Framework.Infrastructure.Storage.Endpoints;

public static class UploadFileEndpoint
{
    internal static RouteHandlerBuilder MapUploadFileEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/upload", async (
            FileUploadCommand command,
            IValidator<FileUploadCommand> validator,
            IStorageService storageService,
            CancellationToken cancellationToken) =>
        {
            ValidationResult result = await validator.ValidateAsync(command, cancellationToken);
            if (!result.IsValid)
            {
                return Results.ValidationProblem(result.ToDictionary());
            }

            var fileType = DetermineFileType(command.Extension);
            var uri = await storageService.UploadAsync<object>(command, fileType, cancellationToken);
            
            if (uri == null)
            {
                return Results.BadRequest("File upload failed.");
            }
            
            return Results.Ok(new FileUploadResponse { Url = uri });
        })
        .WithName(nameof(UploadFileEndpoint))
        .WithSummary("Upload a file")
        .WithDescription("Uploads a file to the storage service.")
        .RequirePermission($"Permissions.{FshResources.Files}.{FshActions.Create}");
    }

    private static FileType DetermineFileType(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return FileType.Other;
        }

        var lowerExtension = extension.ToLowerInvariant();

        // Image files
        if (new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".svg", ".webp" }.Contains(lowerExtension))
        {
            return FileType.Image;
        }

        // Document files
        if (new[] { ".pdf", ".doc", ".docx", ".txt", ".xlsx", ".xls", ".csv" }.Contains(lowerExtension))
        {
            return FileType.Document;
        }

        // Video files
        if (new[] { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv" }.Contains(lowerExtension))
        {
            return FileType.Video;
        }

        // Audio files
        if (new[] { ".mp3", ".wav", ".flac", ".aac", ".ogg" }.Contains(lowerExtension))
        {
            return FileType.Audio;
        }

        return FileType.Other;
    }
}
