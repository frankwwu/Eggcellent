using Eggcellent.Common;

namespace Eggcellent.Models
{
    public class InstalledAppItem : ViewModelBase
    {
        public string Name { get; }
        public string Publisher { get; }
        public string Version { get; }
        public long EstimatedSizeBytes { get; }
        public string SizeDisplay => EstimatedSizeBytes > 0 ? ByteFormatter.Format(EstimatedSizeBytes) : "—";
        public string UninstallString { get; }
        public string? InstallLocation { get; }

        public InstalledAppItem(string name, string publisher, string version, long estimatedSizeBytes, string uninstallString, string? installLocation)
        {
            Name = name;
            Publisher = publisher;
            Version = version;
            EstimatedSizeBytes = estimatedSizeBytes;
            UninstallString = uninstallString;
            InstallLocation = installLocation;
        }
    }
}
