// Services/FileComparisonService.cs
using ServerDeploymentTool.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ServerDeploymentTool.Services
{
    public class FileComparisonService : IFileComparisonService
    {
        public async Task<List<FileComparisonResult>> CompareFilesAsync(string localPath, string remotePath, ISshService sshService)
        {
            var results = new List<FileComparisonResult>();

            // Get all local files
            var localFiles = Directory.GetFiles(localPath, "*.*", SearchOption.AllDirectories);

            // Get all remote files
            var remoteFiles = await sshService.GetRemoteFilesAsync(remotePath);

            // Compare local files with remote
            foreach (var localFile in localFiles)
            {
                var relativePath = localFile.Substring(localPath.Length + 1).Replace("\\", "/");
                var remoteFilePath = $"{remotePath}/{relativePath}";

                var localHash = CalculateLocalFileHash(localFile);
                var fileInfo = new FileInfo(localFile);

                var result = new FileComparisonResult
                {
                    FilePath = relativePath,
                    LocalHash = localHash,
                    FileSize = fileInfo.Length,
                    LastModified = fileInfo.LastWriteTime
                };

                if (remoteFiles.Contains(remoteFilePath))
                {
                    result.Exists = true;
                    result.RemoteHash = await sshService.GetFileHashAsync(remoteFilePath);
                    result.IsModified = !string.Equals(localHash, result.RemoteHash, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    result.Exists = false;
                    result.IsModified = true;
                }

                results.Add(result);
            }

            return results;
        }

        public string CalculateLocalFileHash(string filePath)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filePath);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
