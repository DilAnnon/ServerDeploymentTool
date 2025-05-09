// Services/IFileComparisonService.cs
using ServerDeploymentTool.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerDeploymentTool.Services
{
    public interface IFileComparisonService
    {
        Task<List<FileComparisonResult>> CompareFilesAsync(string localPath, string remotePath, ISshService sshService);
        string CalculateLocalFileHash(string filePath);
    }
}
