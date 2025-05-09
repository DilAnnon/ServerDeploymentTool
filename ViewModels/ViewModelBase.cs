// ViewModels/ViewModelBase.cs
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;

namespace ServerDeploymentTool.ViewModels
{
    public abstract class ViewModelBase : ObservableObject
    {
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        protected async Task ExecuteWithBusyIndicatorAsync(Func<Task> action, string busyMessage = "Working...")
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                StatusMessage = busyMessage;
                await action();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
