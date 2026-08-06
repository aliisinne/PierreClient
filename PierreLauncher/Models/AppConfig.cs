using System.Collections.Generic;

namespace PierreLauncher.Models
{
    public class AppConfig
    {
        public List<Account> Accounts { get; set; } = new List<Account>();
        public string ActiveUsername { get; set; } = string.Empty;
        
        public double RamAllocationGb { get; set; } = 4.0;
        public string SelectedJava { get; set; } = "Java 21 (Varsayılan)";
        public string SelectedLanguage { get; set; } = "Türkçe";
        public string SelectedBranch { get; set; } = "master";
        public bool MinimizeOnLaunch { get; set; } = true;
        public bool CloseOnLaunch { get; set; } = false;

        public string JavaPath { get; set; } = string.Empty;
        public string GameDirectory { get; set; } = string.Empty;
        public string JvmArguments { get; set; } = "-XX:+UnlockExperimentalVMOptions -XX:+UseG1GC";
        public string TargetVersion { get; set; } = "1.21.1";
    }
}
