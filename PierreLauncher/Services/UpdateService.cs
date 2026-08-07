using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace PierreLauncher.Services
{
    public class VersionInfo
    {
        [JsonProperty("latestVersion")]
        public string LatestVersion { get; set; } = "1.0.0";

        [JsonProperty("downloadUrl")]
        public string DownloadUrl { get; set; } = "";

        [JsonProperty("mandatory")]
        public bool Mandatory { get; set; } = false;
    }

    public class NewsItem
    {
        [JsonProperty("titleTr")]
        public string TitleTr { get; set; } = "";
        [JsonProperty("titleEn")]
        public string TitleEn { get; set; } = "";
        
        [JsonProperty("descTr")]
        public string DescTr { get; set; } = "";
        [JsonProperty("descEn")]
        public string DescEn { get; set; } = "";
        
        [JsonProperty("dateTr")]
        public string DateTr { get; set; } = "";
        [JsonProperty("dateEn")]
        public string DateEn { get; set; } = "";
    }

    public class UpdateService
    {
        public static List<NewsItem> GlobalNews { get; set; } = new List<NewsItem>();
        
        public static readonly string CurrentVersion = "1.1.0";
        private const string VersionUrl = "https://raw.githubusercontent.com/aliisinne/PierreClient/master/version.json";
        private const string NewsUrl = "https://raw.githubusercontent.com/aliisinne/PierreClient/master/news.json";

        private readonly HttpClient _httpClient;

        public UpdateService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "PierreLauncher");
        }

        public async Task<VersionInfo?> CheckForUpdatesAsync()
        {
            try
            {
                var json = await _httpClient.GetStringAsync(VersionUrl);
                return JsonConvert.DeserializeObject<VersionInfo>(json);
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<NewsItem>> FetchNewsAsync()
        {
            try
            {
                var json = await _httpClient.GetStringAsync(NewsUrl);
                var items = JsonConvert.DeserializeObject<List<NewsItem>>(json);
                return items ?? new List<NewsItem>();
            }
            catch
            {
                return new List<NewsItem>();
            }
        }

        public async Task<bool> DownloadUpdateAsync(string downloadUrl, string destinationPath)
        {
            try
            {
                var response = await _httpClient.GetAsync(downloadUrl);
                response.EnsureSuccessStatusCode();

                using var fs = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await response.Content.CopyToAsync(fs);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void ApplyUpdateAndRestart(string newExePath)
        {
            string batPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update_installer.bat");
            string currentExe = Process.GetCurrentProcess().MainModule?.FileName ?? "PierreLauncher.exe";

            string batContent = $@"
@echo off
timeout /t 2 /nobreak > nul
move /y ""{newExePath}"" ""{currentExe}""
start """" ""{currentExe}""
del ""%~f0""
";
            File.WriteAllText(batPath, batContent);

            var psi = new ProcessStartInfo
            {
                FileName = batPath,
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process.Start(psi);
            Application.Current.Shutdown();
        }
    }
}
