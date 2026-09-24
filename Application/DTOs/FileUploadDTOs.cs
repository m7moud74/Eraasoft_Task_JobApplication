using System.IO;

namespace JobApplication.Application.DTOs;

public record FileUploadRequest(Stream FileStream, string FileName, string ContentType, long Length);

public record FileUploadResult(string Url, string PublicId);
