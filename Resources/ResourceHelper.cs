// Properties/ResourceHelper.cs
using System.Globalization;
using System.Resources;

namespace ServerDeploymentTool.Properties
{
    public static class ResourceHelper
    {
        private static readonly ResourceManager ResourceManager = new ResourceManager(
            "ServerDeploymentTool.Properties.Resources",
            typeof(Resources).Assembly);

        public static string GetString(string key)
        {
            return ResourceManager1.GetString(key, CultureInfo.CurrentUICulture) ?? key;
        }
    }
}
