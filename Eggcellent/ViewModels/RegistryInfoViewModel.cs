using Eggcellent.Common;
using Eggcellent.Services;

namespace Eggcellent.ViewModels
{
    public class RegistryInfoViewModel : ViewModelBase
    {
        private string _status = "";
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public RelayCommand OpenRegistryEditorCommand { get; }

        public RegistryInfoViewModel()
        {
            OpenRegistryEditorCommand = new RelayCommand(() =>
                Status = ToolboxService.OpenRegistryEditor()
                    ? "Registry Editor opened. Review changes carefully — consider exporting a backup first (File > Export)."
                    : "Could not open Registry Editor.");
        }
    }
}
