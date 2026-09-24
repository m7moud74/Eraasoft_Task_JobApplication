using System.Threading;
using System.Threading.Tasks;
using JobApplication.Application.DTOs;

namespace JobApplication.Application.Interfaces;

public interface IFileStorageService
{
    Task<FileUploadResult> UploadFileAsync(FileUploadRequest request, string folder = "cvs", CancellationToken cancellationToken = default);
    Task<bool> DeleteFileAsync(string publicId, CancellationToken cancellationToken = default);
}
