// Models/FileComparisonResult.cs
using System;

namespace ServerDeploymentTool.Models
{
    public class FileComparisonResult
    {
        public string FilePath { get; set; } = string.Empty;
        public bool Exists { get; set; }
        public bool IsModified { get; set; }
        public string LocalHash { get; set; } = string.Empty;
        public string RemoteHash { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime LastModified { get; set; }
    }
}
