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

        byte[] fileBytes;
        if (request.FileStream is MemoryStream ms)
        {
            fileBytes = ms.ToArray();
        }
        else
        {
            using var buffer = new MemoryStream();
            if (request.FileStream.CanSeek)
            {
                request.FileStream.Position = 0;
            }
            await request.FileStream.CopyToAsync(buffer, cancellationToken);
            fileBytes = buffer.ToArray();
        }

        if (fileBytes.Length == 0)
        {
            throw new BadRequestException("Uploaded file contains no data.");
        }

        var safeFileName = Path.GetFileNameWithoutExtension(request.FileName);
        var publicId = $"{folder}/{Guid.NewGuid():N}_{safeFileName}";

        try
        {
            using var cloudinaryStream = new MemoryStream(fileBytes);
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(request.FileName, cloudinaryStream),
                PublicId = publicId,
                Overwrite = true
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams, "auto", cancellationToken);
            if (uploadResult.Error == null)
            {
                var secureUrl = uploadResult.SecureUrl?.ToString() ?? uploadResult.Url?.ToString() ?? string.Empty;
                return new FileUploadResult(secureUrl, uploadResult.PublicId);
            }

            _logger.LogWarning("Cloudinary upload failed ({Error}). Falling back to local storage.", uploadResult.Error.Message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cloudinary upload encountered an exception. Falling back to local storage.");
        }

        // Resilient fallback to local disk storage
        return await SaveLocallyAsync(fileBytes, request.FileName, folder, cancellationToken);
    }

    private async Task<FileUploadResult> SaveLocallyAsync(byte[] fileBytes, string originalFileName, string folder, CancellationToken cancellationToken)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", folder);
        Directory.CreateDirectory(basePath);

        var safeExtension = Path.GetExtension(originalFileName);
        var uniqueFileName = $"{Guid.NewGuid():N}{safeExtension}";
        var fullPath = Path.Combine(basePath, uniqueFileName);

        await File.WriteAllBytesAsync(fullPath, fileBytes, cancellationToken);

        var relativeUrl = $"/uploads/{folder}/{uniqueFileName}";
        var publicId = $"local:{folder}/{uniqueFileName}";

        _logger.LogInformation("File saved to local storage fallback at {Path}", fullPath);
        return new FileUploadResult(relativeUrl, publicId);
    }

    public async Task<bool> DeleteFileAsync(string publicId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(publicId))
        {
            return false;
        }

        if (publicId.StartsWith("local:", StringComparison.OrdinalIgnoreCase))
        {
            var relativePath = publicId.Substring("local:".Length);
            var localPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", relativePath);
            if (File.Exists(localPath))
            {
                File.Delete(localPath);
            }
            return true;
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
