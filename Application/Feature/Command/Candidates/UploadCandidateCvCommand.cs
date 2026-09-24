using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using MediatR;

namespace JobApplication.Application.Feature.Command.Candidates;

public record UploadCandidateCvCommand(
    int CandidateId,
    Stream FileStream,
    string FileName,
    string ContentType,
    long Length) : IRequest<CandidateDto>;

public class UploadCandidateCvCommandHandler : IRequestHandler<UploadCandidateCvCommand, CandidateDto>
{
    private const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5 MB
    private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx" };
    private static readonly string[] AllowedMimeTypes =
    {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/octet-stream"
    };

    private readonly ICandidateRepository _candidateRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;

    public UploadCandidateCvCommandHandler(
        ICandidateRepository candidateRepository,
        IFileStorageService fileStorageService,
        ICurrentUserService currentUserService)
    {
        _candidateRepository = candidateRepository;
        _fileStorageService = fileStorageService;
        _currentUserService = currentUserService;
    }

    public async Task<CandidateDto> Handle(UploadCandidateCvCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAdmin && _currentUserService.CandidateId != request.CandidateId)
        {
            throw new ForbiddenAccessException("You are only authorized to upload a CV for your own profile.");
        }

        var candidate = await _candidateRepository.GetByIdAsync(request.CandidateId, cancellationToken);
        if (candidate is null)
        {
            throw new NotFoundException($"Candidate with ID {request.CandidateId} was not found.");
        }

        ValidateFile(request);

        // Delete old CV from Cloudinary if previously uploaded to avoid orphaned files
        if (!string.IsNullOrWhiteSpace(candidate.CvPublicId))
        {
            await _fileStorageService.DeleteFileAsync(candidate.CvPublicId, cancellationToken);
        }

        var uploadRequest = new FileUploadRequest(request.FileStream, request.FileName, request.ContentType, request.Length);
        var uploadResult = await _fileStorageService.UploadFileAsync(uploadRequest, "candidate_cvs", cancellationToken);

        candidate.CvUrl = uploadResult.Url;
        candidate.CvPublicId = uploadResult.PublicId;

        _candidateRepository.Update(candidate);
        await _candidateRepository.SaveChangesAsync(cancellationToken);

        return new CandidateDto
        {
            Id = candidate.Id,
            Name = candidate.Name,
            Email = candidate.Email,
            CvUrl = candidate.CvUrl,
            CvPublicId = candidate.CvPublicId
        };
    }

    private static void ValidateFile(UploadCandidateCvCommand request)
    {
        if (request.FileStream == null || request.Length == 0)
        {
            throw new BadRequestException("Uploaded file is empty.");
        }

        if (request.Length > MaxFileSizeInBytes)
        {
            throw new BadRequestException($"File size exceeds the maximum allowed limit of {MaxFileSizeInBytes / (1024 * 1024)} MB.");
        }

        var extension = Path.GetExtension(request.FileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new BadRequestException($"Unsupported file type '{extension}'. Allowed types: {string.Join(", ", AllowedExtensions)}.");
        }

        if (!string.IsNullOrEmpty(request.ContentType) &&
            !AllowedMimeTypes.Contains(request.ContentType.ToLowerInvariant()))
        {
            throw new BadRequestException($"Invalid Content-Type '{request.ContentType}'.");
        }

        // Validate file signature (magic bytes)
        ValidateMagicBytes(request.FileStream, extension);
    }

    private static void ValidateMagicBytes(Stream stream, string extension)
    {
        if (!stream.CanSeek)
        {
            return;
        }

        var originalPosition = stream.Position;
        try
        {
            stream.Position = 0;
            var header = new byte[8];
            var bytesRead = stream.Read(header, 0, header.Length);
            if (bytesRead < 4)
            {
                throw new BadRequestException("Corrupted or invalid file header.");
            }

            if (extension == ".pdf")
            {
                // PDF header: %PDF (0x25, 0x50, 0x44, 0x46)
                if (header[0] != 0x25 || header[1] != 0x50 || header[2] != 0x44 || header[3] != 0x46)
                {
                    throw new BadRequestException("File content does not match a valid PDF document.");
                }
            }
            else if (extension == ".docx")
            {
                // DOCX is a zip: PK.. (0x50, 0x4B, 0x03, 0x04)
                if (header[0] != 0x50 || header[1] != 0x4B)
                {
                    throw new BadRequestException("File content does not match a valid DOCX document.");
                }
            }
            else if (extension == ".doc")
            {
                // DOC header: 0xD0, 0xCF, 0x11, 0xE0
                if (header[0] != 0xD0 || header[1] != 0xCF || header[2] != 0x11 || header[3] != 0xE0)
                {
                    throw new BadRequestException("File content does not match a valid DOC document.");
                }
            }
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }
}
