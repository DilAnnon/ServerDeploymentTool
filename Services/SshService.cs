// Services/SshService.cs
using Renci.SshNet;
using Renci.SshNet.Sftp;
using ServerDeploymentTool.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerDeploymentTool.Services
{
    public class SshService : ISshService, IDisposable
    {
        private SshClient? _sshClient;
        private SftpClient? _sftpClient;
        private ServerSettings? _currentSettings;

        public async Task<bool> ConnectAsync(ServerSettings settings)
        {
            try
            {
                Disconnect();

                _currentSettings = settings;

                ConnectionInfo connectionInfo;

                if (settings.UsePrivateKey && !string.IsNullOrEmpty(settings.PrivateKeyPath))
                {
                    var privateKeyFile = new PrivateKeyFile(settings.PrivateKeyPath);
                    connectionInfo = new ConnectionInfo(
                        settings.Hostname,
                        settings.Port,
                        settings.Username,
                        new PrivateKeyAuthenticationMethod(settings.Username, privateKeyFile));
                }
                else
                {
                    connectionInfo = new ConnectionInfo(
                        settings.Hostname,
                        settings.Port,
                        settings.Username,
                        new PasswordAuthenticationMethod(settings.Username, settings.Password));
                }

                _sshClient = new SshClient(connectionInfo);
                _sftpClient = new SftpClient(connectionInfo);

                await Task.Run(() => {
                    _sshClient.Connect();
                    _sftpClient.Connect();
                });

                return _sshClient.IsConnected && _sftpClient.IsConnected;
            }
            catch
            {
                return false;
            }
        }

        public void Disconnect()
        {
            _sshClient?.Disconnect();
            _sshClient?.Dispose();
            _sshClient = null;

            _sftpClient?.Disconnect();
            _sftpClient?.Dispose();
            _sftpClient = null;
        }

        public async Task<bool> TestConnectionAsync(ServerSettings settings)
        {
            try
            {
                var connected = await ConnectAsync(settings);
                Disconnect();
                return connected;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> ExecuteCommandAsync(string command)
        {
            if (_sshClient == null || !_sshClient.IsConnected)
                throw new InvalidOperationException("SSH client is not connected");

            return await Task.Run(() => {
                using var cmd = _sshClient.CreateCommand(command);
                var result = cmd.Execute();

                if (cmd.ExitStatus != 0)
                {
                    return $"Error (Exit code: {cmd.ExitStatus}): {cmd.Error}";
                }

                return result;
            });
        }

        public async Task<List<string>> GetRemoteFilesAsync(string remotePath)
        {
            if (_sftpClient == null || !_sftpClient.IsConnected)
                throw new InvalidOperationException("SFTP client is not connected");

            return await Task.Run(() => {
                var result = new List<string>();
                GetRemoteFilesRecursive(remotePath, result);
                return result;
            });
        }

        private void GetRemoteFilesRecursive(string remotePath, List<string> files)
        {
            if (_sftpClient == null) return;

            foreach (var file in _sftpClient.ListDirectory(remotePath))
            {
                if (file.Name == "." || file.Name == "..") continue;

                var fullPath = $"{remotePath}/{file.Name}";

                if (file.IsDirectory)
                {
                    GetRemoteFilesRecursive(fullPath, files);
                }
                else
                {
                    files.Add(fullPath);
                }
            }
        }

        public async Task<string> GetFileHashAsync(string remotePath)
        {
            return await ExecuteCommandAsync($"md5sum \"{remotePath}\" | cut -d ' ' -f 1");
        }

        public async Task UploadDirectoryAsync(string localPath, string remotePath, IProgress<(int current, int total, string fileName)> progress)
        {
            if (_sftpClient == null || !_sftpClient.IsConnected)
                throw new InvalidOperationException("SFTP client is not connected");

            await Task.Run(() => {
                var files = Directory.GetFiles(localPath, "*.*", SearchOption.AllDirectories);
                int total = files.Length;
                int current = 0;

                foreach (var file in files)
                {
                    var relativePath = file.Substring(localPath.Length + 1);
                    var remoteFilePath = Path.Combine(remotePath, relativePath).Replace("\\", "/");
                    var remoteDir = Path.GetDirectoryName(remoteFilePath)?.Replace("\\", "/");

                    if (!string.IsNullOrEmpty(remoteDir))
                    {
                        CreateRemoteDirectory(remoteDir);
                    }

                    using var fileStream = File.OpenRead(file);
                    _sftpClient.UploadFile(fileStream, remoteFilePath);

                    current++;
                    progress.Report((current, total, relativePath));
                }
            });
        }

        private void CreateRemoteDirectory(string path)
        {
            if (_sftpClient == null) return;

            string[] folders = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string currentPath = "/";

            foreach (string folder in folders)
            {
                currentPath = Path.Combine(currentPath, folder).Replace("\\", "/");

                if (!DirectoryExists(currentPath))
                {
                    _sftpClient.CreateDirectory(currentPath);
                }
            }
        }

        private bool DirectoryExists(string path)
        {
            if (_sftpClient == null) return false;

            try
            {
                return _sftpClient.Exists(path) && _sftpClient.GetAttributes(path).IsDirectory;
            }
            catch
            {
                return false;
            }
        }

        public async Task StopApplicationAsync(string appName)
        {
            await ExecuteCommandAsync($"pm2 stop {appName}");
        }

        public async Task StartApplicationAsync(string appName)
        {
            await ExecuteCommandAsync($"pm2 start {appName}");
        }

        public async Task DeleteRemoteDirectoryAsync(string remotePath)
        {
            await ExecuteCommandAsync($"rm -rf \"{remotePath}\"");
        }

        public async Task RunNpmInstallAsync(string remotePath)
        {
            await ExecuteCommandAsync($"cd \"{remotePath}\" && npm install");
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
