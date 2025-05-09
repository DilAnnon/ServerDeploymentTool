// Services/ISshService.cs
using ServerDeploymentTool.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerDeploymentTool.Services
{
    public interface ISshService
    {
        Task<bool> TestConnectionAsync(ServerSettings settings);
        Task<string> ExecuteCommandAsync(string command);
        Task<List<string>> GetRemoteFilesAsync(string remotePath);
        Task<string> GetFileHashAsync(string remotePath);
        Task UploadDirectoryAsync(string localPath, string remotePath, IProgress<(int current, int total, string fileName)> progress);
        Task StopApplicationAsync(string appName);
        Task StartApplicationAsync(string appName);
        Task DeleteRemoteDirectoryAsync(string remotePath);
        Task RunNpmInstallAsync(string remotePath);
        Task<bool> ConnectAsync(ServerSettings settings);
        void Disconnect();
    }
}
