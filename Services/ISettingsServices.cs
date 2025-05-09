// Services/ISettingsService.cs
using ServerDeploymentTool.Models;
using System.Threading.Tasks;

namespace ServerDeploymentTool.Services
{
    public interface ISettingsService
    {
        Task<AppSettings> LoadSettingsAsync();
        Task SaveSettingsAsync(AppSettings settings);
    }
}
