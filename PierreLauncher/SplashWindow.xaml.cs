using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.IO.Compression;
using System.Net.Http;
using System.Linq;
using PierreLauncher.Services;

namespace PierreLauncher
{
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent();
            this.Loaded += SplashWindow_Loaded;
        }

        private async void SplashWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (Resources["FadeInStoryboard"] is Storyboard fadeIn)
            {
                fadeIn.Begin(this);
            }

            await PerformStartupChecksAsync();

            if (Resources["FadeOutStoryboard"] is Storyboard fadeOut)
            {
                fadeOut.Completed += (s, ev) =>
                {
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                };
                fadeOut.Begin(this);
            }
            else
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
        }

        private async Task PerformStartupChecksAsync()
        {
            await UpdateProgressAsync("Başlatıcı Hazırlanıyor...", 10, 200);

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string minecraftDir = Path.Combine(appData, ".minecraft");

            if (!Directory.Exists(minecraftDir))
            {
                await UpdateProgressAsync("Minecraft klasörü oluşturuluyor...", 30, 500);
                Directory.CreateDirectory(minecraftDir);
            }
            else
            {
                await UpdateProgressAsync("Minecraft dizini bulundu...", 30, 200);
            }

            // Check Internet Connection
            await UpdateProgressAsync("İnternet bağlantısı kontrol ediliyor...", 40, 0);
            try
            {
                using var client = new System.Net.Http.HttpClient();
                client.Timeout = TimeSpan.FromSeconds(3);
                await client.GetAsync("https://meta.fabricmc.net/");
                await UpdateProgressAsync("Bağlantı başarılı.", 50, 200);

                // Auto-Updater and News Fetch
                await UpdateProgressAsync("Güncellemeler kontrol ediliyor...", 55, 0);
                var updateService = new UpdateService();
                
                var versionInfo = await updateService.CheckForUpdatesAsync();
                if (versionInfo != null && versionInfo.LatestVersion != UpdateService.CurrentVersion)
                {
                    await UpdateProgressAsync("Yeni güncelleme bulundu. İndiriliyor...", 60, 0);
                    string newExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PierreLauncher_Update.exe");
                    bool success = await updateService.DownloadUpdateAsync(versionInfo.DownloadUrl, newExePath);
                    if (success)
                    {
                        await UpdateProgressAsync("Güncelleme indirildi. Yeniden başlatılıyor...", 70, 500);
                        updateService.ApplyUpdateAndRestart(newExePath);
                        return; // Stop execution, app will restart
                    }
                    else
                    {
                        await UpdateProgressAsync("Güncelleme indirilemedi, devam ediliyor...", 65, 500);
                    }
                }

                await UpdateProgressAsync("Haberler alınıyor...", 70, 0);
                UpdateService.GlobalNews = await updateService.FetchNewsAsync();
            }
            catch
            {
                await UpdateProgressAsync("Uyarı: İnternet bağlantısı yok veya zayıf.", 70, 800);
            }

            // Ensure Portable Java 21 exists
            string runtimeDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Runtime");
            string javaDir = Path.Combine(runtimeDir, "Java21");
            string javaExe = Path.Combine(javaDir, "bin", "java.exe");
            
            // On Windows, the extracted adoptium zip usually has a subfolder like `jdk-21...-jre`
            // Let's check if there's any java.exe inside Java21
            bool javaExists = false;
            if (Directory.Exists(javaDir))
            {
                var files = Directory.GetFiles(javaDir, "java.exe", SearchOption.AllDirectories);
                if (files.Length > 0)
                {
                    javaExists = true;
                    javaExe = files[0];
                }
            }

            if (!javaExists)
            {
                await UpdateProgressAsync("Portable Java 21 (JRE) bulunamadı. İndiriliyor...", 75, 0);
                try
                {
                    Directory.CreateDirectory(runtimeDir);
                    string zipPath = Path.Combine(runtimeDir, "jre21.zip");

                    using var httpClient = new HttpClient();
                    httpClient.Timeout = TimeSpan.FromMinutes(5);
                    
                    // Adoptium JRE 21 Windows x64 API URL
                    string javaUrl = "https://api.adoptium.net/v3/binary/latest/21/ga/windows/x64/jre/hotspot/normal/eclipse";
                    
                    using var response = await httpClient.GetAsync(javaUrl, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    long totalBytes = response.Content.Headers.ContentLength ?? 40_000_000L;
                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
                    
                    byte[] buffer = new byte[8192];
                    long totalRead = 0;
                    int bytesRead;
                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        totalRead += bytesRead;
                        double percentage = (double)totalRead / totalBytes * 100.0;
                        
                        // Scale progress between 75 and 90
                        double scaledProgress = 75 + (percentage * 0.15);
                        
                        // Update UI without delay
                        TxtStatus.Text = $"Portable Java 21 İndiriliyor... ({totalRead / 1024 / 1024} MB / {totalBytes / 1024 / 1024} MB)";
                        double targetWidth = 250.0 * (scaledProgress / 100.0);
                        ProgressBar.BeginAnimation(FrameworkElement.WidthProperty, null); // Stop existing animation
                        ProgressBar.Width = targetWidth;
                    }
                    
                    fileStream.Close();
                    
                    await UpdateProgressAsync("Java 21 Arşivi Çıkarılıyor...", 90, 500);
                    if (Directory.Exists(javaDir)) Directory.Delete(javaDir, true);
                    Directory.CreateDirectory(javaDir);
                    ZipFile.ExtractToDirectory(zipPath, javaDir);
                    
                    File.Delete(zipPath);
                    await UpdateProgressAsync("Java 21 başarıyla kuruldu!", 95, 400);
                }
                catch (Exception ex)
                {
                    await UpdateProgressAsync($"Java 21 İndirme Hatası: {ex.Message}", 95, 2000);
                }
            }
            else
            {
                await UpdateProgressAsync("Portable Java 21 hazır.", 90, 200);
            }

            // Finalize
            await UpdateProgressAsync("Ana Menüye Geçiliyor...", 100, 200);
        }

        private async Task UpdateProgressAsync(string message, double progressPercentage, int delayMs)
        {
            TxtStatus.Text = message;
            
            // Animate progress bar width
            double targetWidth = 250.0 * (progressPercentage / 100.0);
            
            var animation = new DoubleAnimation
            {
                To = targetWidth,
                Duration = TimeSpan.FromMilliseconds(400),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            ProgressBar.BeginAnimation(FrameworkElement.WidthProperty, animation);
            
            await Task.Delay(delayMs);
        }
    }
}
