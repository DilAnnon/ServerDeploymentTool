// Models/ServerSettings.cs
using System;

namespace ServerDeploymentTool.Models
{
    public class ServerSettings
    {
        public string ServerPath { get; set; } = string.Empty;
        public string Hostname { get; set; } = string.Empty;
        public int Port { get; set; } = 22;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PrivateKeyPath { get; set; } = string.Empty;
        public bool UsePrivateKey { get; set; } = false;
        public string AppName { get; set; } = string.Empty;
    }
}
