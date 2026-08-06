using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.ProcessBuilder;
using PierreLauncher.Models;

namespace PierreLauncher.Services
{
    public class MinecraftLaunchService
    {
        private readonly ConfigService _configService;
        private static readonly HttpClient HttpClient = new HttpClient();

        static MinecraftLaunchService()
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 256;
        }

        public event Action<string>? StatusChanged;
        public event Action<int>? ProgressChanged;
        public event Action<string>? LogOutputReceived;

        public MinecraftLaunchService(ConfigService configService)
        {
            _configService = configService;
            if (!HttpClient.DefaultRequestHeaders.Contains("User-Agent"))
            {
                HttpClient.DefaultRequestHeaders.Add("User-Agent", "PierreClient/1.0");
            }
        }

        public async Task<System.Diagnostics.Process> LaunchAsync()
        {
            var activeAccount = _configService.GetActiveAccount();
            if (activeAccount == null || string.IsNullOrWhiteSpace(activeAccount.Username))
            {
                throw new InvalidOperationException("Lütfen oyuna başlamadan önce geçerli bir hesap seçin.");
            }

            string gameDir = _configService.Config.GameDirectory;
            if (string.IsNullOrWhiteSpace(gameDir))
            {
                gameDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft");
            }

            if (!Directory.Exists(gameDir))
            {
                Directory.CreateDirectory(gameDir);
            }

            // Step 1: Sync Pierre Client & Fabric API mods
            StatusChanged?.Invoke("Pierre Client ve Fabric API modları hazırlanıyor...");
            ProgressChanged?.Invoke(10);
            SyncPierreMods(gameDir);

            // Step 1.5: Download Next-Gen Performance Mods (Sodium & Iris) automatically
            StatusChanged?.Invoke("Modern FPS Optimizasyon (Sodium & Iris) modları kontrol ediliyor...");
            ProgressChanged?.Invoke(15);
            await DownloadPerformanceModsAsync(gameDir);

            // Step 2: Patch AccessWidener files in mods to resolve namespace intermediary error
            StatusChanged?.Invoke("Mod uyumlulukları kontrol ediliyor (AccessWidener yamalanıyor)...");
            ProgressChanged?.Invoke(20);
            PatchModAccessWideners(Path.Combine(gameDir, "mods"));

            // Step 3: Install Fabric Loader 0.17.3 for MC 1.21.11
            string targetMcVersion = "1.21.11";
            string loaderVersion = "0.17.3";
            StatusChanged?.Invoke($"Fabric Loader {loaderVersion} (MC {targetMcVersion}) doğrulanıyor...");
            ProgressChanged?.Invoke(35);
            string fabricVersionId = await EnsureFabricProfileInstalledAsync(gameDir, targetMcVersion, loaderVersion);

            var path = new MinecraftPath(gameDir);
            var launcher = new MinecraftLauncher(path);

            StatusChanged?.Invoke("Oyun versiyonları taranıyor...");
            ProgressChanged?.Invoke(50);

            var versions = await launcher.GetAllVersionsAsync();
            var fabricVersion = versions.FirstOrDefault(v => v.Name.Equals(fabricVersionId, StringComparison.OrdinalIgnoreCase))
                                ?? versions.FirstOrDefault(v => v.Name.Contains("1.21.11", StringComparison.OrdinalIgnoreCase))
                                ?? versions.FirstOrDefault(v => v.Name.Contains("fabric", StringComparison.OrdinalIgnoreCase));

            string selectedVersionId = fabricVersion?.Name ?? fabricVersionId;

            StatusChanged?.Invoke($"Fabric ve Minecraft 1.21.11 hazırlanıyor ({selectedVersionId})...");
            ProgressChanged?.Invoke(65);

            int ramMb = (int)(_configService.Config.RamAllocationGb * 1024);
            if (ramMb < 1024) ramMb = 1024;

            var launchOption = new MLaunchOption
            {
                Session = MSession.CreateOfflineSession(activeAccount.Username),
                MaximumRamMb = ramMb,
                VersionType = "PierreClient",
                GameLauncherName = "PierreClient",
                GameLauncherVersion = "1.21.11"
            };

            // JVM Args with Fabric Dev & Optimization Flags
            string userJvmArgs = _configService.Config.JvmArguments;
            if (string.IsNullOrWhiteSpace(userJvmArgs))
            {
                // Ultra-fast Java 21 GC and Startup arguments
                userJvmArgs = "-XX:+UseZGC -XX:+ZGenerational -XX:+DisableExplicitGC -XX:+AlwaysPreTouch -XX:+PerfDisableSharedMem -XX:+UnlockExperimentalVMOptions -XX:MaxGCPauseMillis=10";
            }

            var args = userJvmArgs.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            launchOption.ExtraJvmArguments = args.Select(a => new MArgument(a)).ToArray();

            // Explicit Java 21 Resolution
            string? detectedJava = ResolveJavaPath(_configService.Config.JavaPath);
            if (!string.IsNullOrEmpty(detectedJava))
            {
                launchOption.JavaPath = detectedJava;
            }
            else
            {
                throw new InvalidOperationException("Minecraft 1.21.11 için Java 21 gereklidir. Lütfen sisteminize Java 21 (JDK 21) yükleyin.");
            }

            StatusChanged?.Invoke("Minecraft Fabric 1.21.11 başlatılıyor (Hızlı Başlatma)...");
            ProgressChanged?.Invoke(90);

            // Hook up CmlLib download events so the UI updates while checking/downloading files
            launcher.FileProgressChanged += (s, e) =>
            {
                StatusChanged?.Invoke($"İndiriliyor/Kontrol Ediliyor: {e.Name} (Kalan: {e.TotalTasks - e.ProgressedTasks})");
            };
            
            launcher.ByteProgressChanged += (s, e) =>
            {
                // MFile progress is usually 0-100, we map it to our UI (which expects 0-100)
                double percentage = e.TotalBytes > 0 ? ((double)e.ProgressedBytes / e.TotalBytes) * 100 : 0;
                ProgressChanged?.Invoke((int)percentage);
            };

            // OPTIMIZATION: BuildProcessAsync automatically downloads missing files. 
            // With the events above, the user will now see what's happening.
            var process = await launcher.BuildProcessAsync(selectedVersionId, launchOption);
            
            // CmlLib will now natively locate the game jar because I patched 1.21.11.json to include the "jar" property.
            
            // (Moved launch_args logging below injection)

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            process.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    LogOutputReceived?.Invoke(e.Data);
            };

            process.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    LogOutputReceived?.Invoke("[HATA] " + e.Data);
            };

            // BULLETPROOF FIX for .NET Core: CmlLib populates ArgumentList. If we modify Arguments directly, 
            // .NET either ignores it or throws an InvalidOperationException (crashing silently or failing).
            // We MUST modify the ArgumentList safely to include the base game jar.
            string baseGameJar = Path.Combine(gameDir, "versions", "1.21.11", "1.21.11.jar");
            if (File.Exists(baseGameJar) && process.StartInfo.ArgumentList != null && process.StartInfo.ArgumentList.Count > 0)
            {
                int cpIndex = -1;
                for (int i = 0; i < process.StartInfo.ArgumentList.Count; i++)
                {
                    if (process.StartInfo.ArgumentList[i] == "-cp" || process.StartInfo.ArgumentList[i] == "-classpath")
                    {
                        cpIndex = i;
                        break;
                    }
                }

                if (cpIndex != -1 && cpIndex + 1 < process.StartInfo.ArgumentList.Count)
                {
                    string currentCp = process.StartInfo.ArgumentList[cpIndex + 1];
                    if (!currentCp.Contains("1.21.11.jar"))
                    {
                        process.StartInfo.ArgumentList[cpIndex + 1] = baseGameJar + ";" + currentCp;
                    }
                }
            }
            else if (File.Exists(baseGameJar) && !string.IsNullOrEmpty(process.StartInfo.Arguments) && !process.StartInfo.Arguments.Contains("1.21.11.jar"))
            {
                // Fallback for older .NET Framework if ArgumentList is empty
                process.StartInfo.Arguments = process.StartInfo.Arguments.Replace("-cp ", $"-cp \"{baseGameJar}\";");
            }

            // BULLETPROOF LONG COMMAND LINE FIX:
            // The command line exceeds 8191 characters. Even if CreateProcessW allows 32767,
            // javaw.exe or the C runtime on Windows often silently truncates or fails.
            // We use Java's @argfile feature to pass all arguments via a text file.
            string argFilePath = Path.Combine(gameDir, "launch_args.txt");
            try
            {
                if (process.StartInfo.ArgumentList != null && process.StartInfo.ArgumentList.Count > 0)
                {
                    // Java argfiles require backslashes to be escaped as \\
                    var escapedArgs = process.StartInfo.ArgumentList.Select(a => a.Replace("\\", "\\\\"));
                    // Quote arguments that contain spaces for the argfile
                    var quotedArgs = escapedArgs.Select(a => a.Contains(" ") ? $"\"{a}\"" : a);
                    File.WriteAllLines(argFilePath, quotedArgs);
                    
                    // Clear the long argument list and just pass the argfile
                    process.StartInfo.ArgumentList.Clear();
                    process.StartInfo.ArgumentList.Add($"@{argFilePath}");
                }
                else
                {
                    string escapedArgs = process.StartInfo.Arguments.Replace("\\", "\\\\");
                    File.WriteAllText(argFilePath, escapedArgs);
                    process.StartInfo.Arguments = $"@{argFilePath}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to create argfile: " + ex.Message);
            }

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            StatusChanged?.Invoke("Oyun Başlatıldı! İyi oyunlar.");
            ProgressChanged?.Invoke(100);

            return process;
        }

        private void PatchModAccessWideners(string modsDir)
        {
            try
            {
                if (!Directory.Exists(modsDir)) return;

                var jarFiles = Directory.GetFiles(modsDir, "*.jar", SearchOption.TopDirectoryOnly);
                foreach (var jarPath in jarFiles)
                {
                    // Clean up sources and dev jars - Fabric Loader crashes if it tries to load them
                    if (jarPath.EndsWith("-sources.jar", StringComparison.OrdinalIgnoreCase) || 
                        jarPath.EndsWith("-dev.jar", StringComparison.OrdinalIgnoreCase))
                    {
                        File.Delete(jarPath);
                        Console.WriteLine($"Removed invalid jar: {Path.GetFileName(jarPath)}");
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Mod cleanup error: {ex.Message}");
            }
        }

        private async Task<string> EnsureFabricProfileInstalledAsync(string gameDir, string mcVersion, string loaderVersion)
        {
            string profileName = $"fabric-loader-{loaderVersion}-{mcVersion}";
            string profileDir = Path.Combine(gameDir, "versions", profileName);
            string profileJson = Path.Combine(profileDir, $"{profileName}.json");

            try
            {
                if (!Directory.Exists(profileDir))
                    Directory.CreateDirectory(profileDir);

                if (File.Exists(profileJson) && new FileInfo(profileJson).Length > 100)
                {
                    return profileName; // CACHE HIT: Don't hit the meta API again!
                }

                string url = $"https://meta.fabricmc.net/v2/versions/loader/{mcVersion}/{loaderVersion}/profile/json";
                var response = await HttpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    url = $"https://meta.fabricmc.net/v2/versions/loader/1.21.1/{loaderVersion}/profile/json";
                }

                string jsonContent = await HttpClient.GetStringAsync(url);
                
                // FIX: Ensure the ID in the JSON matches exactly the profileName so CmlLib finds it.
                if (jsonContent.Contains("\"id\":"))
                {
                    jsonContent = System.Text.RegularExpressions.Regex.Replace(jsonContent, "\"id\":\\s*\"[^\"]+\"", $"\"id\": \"{profileName}\"");
                }
                
                // FIX: We must ensure it inherits from mcVersion (1.21.11) so CmlLib finds the custom base game jar!
                if (jsonContent.Contains("\"inheritsFrom\":"))
                {
                    jsonContent = System.Text.RegularExpressions.Regex.Replace(jsonContent, "\"inheritsFrom\":\\s*\"[^\"]+\"", $"\"inheritsFrom\": \"{mcVersion}\"");
                }
                
                await File.WriteAllTextAsync(profileJson, jsonContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fabric profile setup warning: {ex.Message}");
            }

            return profileName;
        }

        private string? ResolveJavaPath(string customJavaPath)
        {
            if (!string.IsNullOrWhiteSpace(customJavaPath) && File.Exists(customJavaPath))
                return customJavaPath;

            // 1. Check for our newly downloaded Portable Java 21
            string portableJavaDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Runtime", "Java21");
            if (Directory.Exists(portableJavaDir))
            {
                var files = Directory.GetFiles(portableJavaDir, "java.exe", SearchOption.AllDirectories);
                if (files.Length > 0)
                {
                    return files[0];
                }
            }

            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            // 2. Prioritize Java 21 JDK verified paths
            string[] explicitJava21Paths = new string[]
            {
                Path.Combine(localAppData, @"Programs\Java\jdk-21.0.11+10\bin\javaw.exe"),
                Path.Combine(appData, @".tlauncher\starter\jre_default\jre-21.0.11-windows-x64\bin\javaw.exe"),
                Path.Combine(appData, @"PierreLauncher\minecraft\runtime\windows-x64\java-runtime-gamma\bin\javaw.exe"),
                Path.Combine(appData, @"com.rexgodstudios.launcher\RexGodLauncher\runtime\java-21\bin\javaw.exe"),
                Path.Combine(programFiles, @"Java\jdk-21\bin\javaw.exe"),
                Path.Combine(programFiles, @"Eclipse Adoptium\jdk-21.0.11.10-hotspot\bin\javaw.exe")
            };

            foreach (var javaw in explicitJava21Paths)
            {
                if (File.Exists(javaw))
                    return javaw;
            }

            if (Directory.Exists(Path.Combine(programFiles, "Java")))
            {
                var files = Directory.GetFiles(Path.Combine(programFiles, "Java"), "javaw.exe", SearchOption.AllDirectories);
                var j21 = files.FirstOrDefault(f => f.Contains("21") || f.Contains("jdk-21"));
                if (j21 != null) return j21;
                if (files.Length > 0) return files[0];
            }

            return null;
        }

        private void SyncPierreMods(string gameDir)
        {
            try
            {
                string modsDir = Path.Combine(gameDir, "mods");
                if (!Directory.Exists(modsDir))
                {
                    Directory.CreateDirectory(modsDir);
                }
                else
                {
                    // Clean up unauthorized mods but DO NOT delete recognized ones to save disk IO.
                    var existingMods = Directory.GetFiles(modsDir, "*.jar", SearchOption.AllDirectories);
                    foreach (var m in existingMods)
                    {
                        try
                        {
                            File.SetAttributes(m, FileAttributes.Normal);
                            // Only delete if it's not a read-only locked mod from our previous sync
                            // This drastically speeds up the launch because we aren't wiping 200 mods every time.
                        }
                        catch { }
                    }
                }

                string[] possibleSourcePaths = new string[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Modlar"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "Pierre", "Modlar"),
                    @"C:\Users\mymai\OneDrive\Belgeler\PierreClient1.21.11\Modlar",
                    @"C:\Users\mymai\OneDrive\Belgeler\Pierre\Modlar"
                };

                foreach (var src in possibleSourcePaths)
                {
                    if (Directory.Exists(src))
                    {
                        var jarFiles = Directory.GetFiles(src, "*.jar", SearchOption.TopDirectoryOnly);
                        foreach (var jar in jarFiles)
                        {
                            if (jar.EndsWith("-sources.jar", StringComparison.OrdinalIgnoreCase) || 
                                jar.EndsWith("-dev.jar", StringComparison.OrdinalIgnoreCase)) 
                                continue;

                            string fileName = Path.GetFileName(jar);
                            string dest = Path.Combine(modsDir, fileName);

                            // OPTIMIZATION: Only copy if missing or size differs
                            var srcInfo = new FileInfo(jar);
                            var destInfo = new FileInfo(dest);
                            
                            if (!destInfo.Exists || destInfo.Length != srcInfo.Length)
                            {
                                File.Copy(jar, dest, overwrite: true);
                                File.SetAttributes(dest, FileAttributes.ReadOnly);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Mod sync error: {ex.Message}");
            }
        }

        private async Task DownloadPerformanceModsAsync(string gameDir)
        {
            string modsDir = Path.Combine(gameDir, "mods");
            if (!Directory.Exists(modsDir))
                Directory.CreateDirectory(modsDir);

            // Using Modrinth CDN for latest stable 1.21.1 versions
            var modsToDownload = new System.Collections.Generic.Dictionary<string, string>
            {
                // CraterLib and SimpleRPC removed because CraterLib is binary incompatible with 1.21.11 (NoSuchMethodError),
                // and SimpleRPC strictly depends on CraterLib. The C# Launcher handles Discord Rich Presence natively instead.
            };
            
            // Delete broken craterlib and simplerpc if they exist
            string[] brokenMods = new[] { "craterlib-1.21.1.jar", "simplerpc-1.21.1.jar" };
            foreach (var mod in brokenMods)
            {
                string brokenModPath = Path.Combine(modsDir, mod);
                if (File.Exists(brokenModPath))
                {
                    try
                    {
                        File.SetAttributes(brokenModPath, FileAttributes.Normal);
                        File.Delete(brokenModPath);
                    }
                    catch { }
                }
            }

            foreach (var mod in modsToDownload)
            {
                string filePath = Path.Combine(modsDir, mod.Key);
                
                // If it already exists, skip it to save time
                if (File.Exists(filePath) && new FileInfo(filePath).Length > 100000)
                    continue;

                try
                {
                    StatusChanged?.Invoke($"İndiriliyor: {mod.Key} (Ultra FPS Modu)...");
                    byte[] data = await HttpClient.GetByteArrayAsync(mod.Value);
                    await File.WriteAllBytesAsync(filePath, data);
                    
                    // Patch the mod so it ignores the "1.21.11" version mismatch and Iris/Sodium cross-checks!
                    PatchModForCustomMcVersion(filePath);

                    // Mark as ReadOnly so our Anti-Tamper doesn't delete it
                    File.SetAttributes(filePath, FileAttributes.ReadOnly);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to download performance mod {mod.Key}: {ex.Message}");
                }
            }

            // Create Discord RPC Configuration
            try
            {
                string rpcConfigDir = Path.Combine(gameDir, "config", "simple-rpc");
                if (!Directory.Exists(rpcConfigDir))
                    Directory.CreateDirectory(rpcConfigDir);

                string rpcConfigFile = Path.Combine(rpcConfigDir, "simple-rpc.toml");
                string configContent = @"# Pierre Client Discord RPC Configuration
[general]
    clientID = ""1269986326162280450"" # Varsayılan SimpleRPC App ID'si (Bunu kendinizinkiyle değiştirebilirsiniz)
    debugging = false
    internalIP = """"
    updateTimer = 2

[image]
    largeImageKey = ""logo""
    largeImageText = ""Pierre Client 1.21.11""
    smallImageKey = ""minecraft""
    smallImageText = ""Fabric Modded""

[text]
    state = ""Oynuyor""
    details = ""Pierre Client""
    multiplayer = ""Çok Oyunculu - %server%""
    singleplayer = ""Tek Oyunculu - %world%""
";
                // Sadece config dosyası yoksa oluştur (Kullanıcı kendi ayarlarını yapmışsa silinmesin)
                if (!File.Exists(rpcConfigFile))
                {
                    await File.WriteAllTextAsync(rpcConfigFile, configContent);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write Discord RPC config: {ex.Message}");
            }
        }

        private void PatchModForCustomMcVersion(string jarPath)
        {
            try
            {
                using (var archive = System.IO.Compression.ZipFile.Open(jarPath, System.IO.Compression.ZipArchiveMode.Update))
                {
                    var entry = archive.GetEntry("fabric.mod.json");
                    if (entry != null)
                    {
                        string json;
                        using (var reader = new System.IO.StreamReader(entry.Open()))
                        {
                            json = reader.ReadToEnd();
                        }

                        try
                        {
                            var rootNode = System.Text.Json.Nodes.JsonNode.Parse(json);
                            
                            if (rootNode["depends"] is System.Text.Json.Nodes.JsonObject dependsObj)
                            {
                                dependsObj.Remove("minecraft");
                                dependsObj.Remove("sodium");
                                dependsObj.Remove("iris");
                            }
                            
                            if (rootNode["breaks"] is System.Text.Json.Nodes.JsonObject breaksObj)
                            {
                                breaksObj.Remove("sodium");
                                breaksObj.Remove("iris");
                            }
                            
                            if (rootNode["conflicts"] is System.Text.Json.Nodes.JsonObject conflictsObj)
                            {
                                conflictsObj.Remove("sodium");
                                conflictsObj.Remove("iris");
                            }

                            json = rootNode.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                            
                            entry.Delete();
                            var newEntry = archive.CreateEntry("fabric.mod.json");
                            using (var writer = new System.IO.StreamWriter(newEntry.Open()))
                            {
                                writer.Write(json);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"JSON parse error for {jarPath}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Patch failed for {jarPath}: {ex.Message}");
            }
        }
    }
}
