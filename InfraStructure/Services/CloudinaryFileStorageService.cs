using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JobApplication.Infrastructure.Services;

public class CloudinaryFileStorageService : IFileStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryFileStorageService> _logger;

    public CloudinaryFileStorageService(IOptions<CloudinarySettings> options, ILogger<CloudinaryFileStorageService> logger)
    {
        _logger = logger;
        var settings = options.Value;

        if (string.IsNullOrWhiteSpace(settings.CloudName) ||
            string.IsNullOrWhiteSpace(settings.ApiKey) ||
            string.IsNullOrWhiteSpace(settings.ApiSecret))
        {
            _logger.LogWarning("Cloudinary credentials are not fully configured. File upload may fail.");
        }

        var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    public async Task<FileUploadResult> UploadFileAsync(FileUploadRequest request, string folder = "cvs", CancellationToken cancellationToken = default)
    {
        if (request.FileStream == null || request.Length == 0)
        {
            throw new BadRequestException("Cannot upload an empty file.");
        }

        if (request.FileStream.CanSeek)
        {
            request.FileStream.Position = 0;
        }

        var safeFileName = Path.GetFileNameWithoutExtension(request.FileName);
        var publicId = $"{folder}/{Guid.NewGuid():N}_{safeFileName}";

        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(request.FileName, request.FileStream),
            PublicId = publicId,
            Overwrite = true
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams, "auto", cancellationToken);
        if (uploadResult.Error != null)
        {
            _logger.LogError("Cloudinary upload failed: {Error}", uploadResult.Error.Message);
            throw new BadRequestException($"Failed to upload file to storage: {uploadResult.Error.Message}");
        }

        var secureUrl = uploadResult.SecureUrl?.ToString() ?? uploadResult.Url?.ToString() ?? string.Empty;

        return new FileUploadResult(secureUrl, uploadResult.PublicId);
    }

    public async Task<bool> DeleteFileAsync(string publicId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(publicId))
        {
            return false;
        }

        try
        {
            var deletionParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Raw
            };

            var result = await _cloudinary.DestroyAsync(deletionParams);
            return result.Result == "ok";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete asset from Cloudinary: {PublicId}", publicId);
            return false;
        }
    }
}
