using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using PierreLauncher.Models;

namespace PierreLauncher.Services
{
    public class ConfigService
    {
        private static readonly string ConfigDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PierreClient"
        );
        private static readonly string ConfigPath = Path.Combine(ConfigDir, "config.json");

        public AppConfig Config { get; private set; } = new AppConfig();

        public ConfigService()
        {
            Load();
        }

        public void Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    var loaded = JsonConvert.DeserializeObject<AppConfig>(json);
                    if (loaded != null)
                    {
                        Config = loaded;
                    }
                }
            }
            catch
            {
                Config = new AppConfig();
            }

            if (string.IsNullOrEmpty(Config.GameDirectory))
            {
                Config.GameDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    ".minecraft"
                );
            }

            if (string.IsNullOrEmpty(Config.JavaPath) || !File.Exists(Config.JavaPath))
            {
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string defaultJ21 = Path.Combine(localAppData, @"Programs\Java\jdk-21.0.11+10\bin\javaw.exe");
                if (!File.Exists(defaultJ21))
                {
                    defaultJ21 = Path.Combine(appData, @".tlauncher\starter\jre_default\jre-21.0.11-windows-x64\bin\javaw.exe");
                }
                if (File.Exists(defaultJ21))
                {
                    Config.JavaPath = defaultJ21;
                }
            }
        }

        public void Save()
        {
            try
            {
                if (!Directory.Exists(ConfigDir))
                    Directory.CreateDirectory(ConfigDir);

                string json = JsonConvert.SerializeObject(Config, Formatting.Indented);
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving config: {ex.Message}");
            }
        }

        public void AddAccount(string username, string type = "Çevrimdışı")
        {
            if (string.IsNullOrWhiteSpace(username)) return;

            username = username.Trim();
            var existing = Config.Accounts.FirstOrDefault(a => a.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            if (existing == null)
            {
                existing = new Account { Username = username, AccountType = type, CreatedAt = DateTime.Now, LastLoginTime = DateTime.Now };
                Config.Accounts.Add(existing);
            }
            else
            {
                existing.LastLoginTime = DateTime.Now;
                existing.AccountType = type;
            }

            SetActiveAccount(username);
        }

        public void SetActiveAccount(string username)
        {
            foreach (var acc in Config.Accounts)
            {
                acc.IsActive = acc.Username.Equals(username, StringComparison.OrdinalIgnoreCase);
                if (acc.IsActive) acc.LastLoginTime = DateTime.Now;
            }
            Config.ActiveUsername = username;
            Save();
        }

        public Account? GetActiveAccount()
        {
            return Config.Accounts.FirstOrDefault(a => a.IsActive) 
                ?? Config.Accounts.FirstOrDefault(a => a.Username.Equals(Config.ActiveUsername, StringComparison.OrdinalIgnoreCase))
                ?? Config.Accounts.FirstOrDefault();
        }

        public void RemoveAccount(string username)
        {
            Config.Accounts.RemoveAll(a => a.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            if (Config.ActiveUsername.Equals(username, StringComparison.OrdinalIgnoreCase))
            {
                var next = Config.Accounts.FirstOrDefault();
                Config.ActiveUsername = next?.Username ?? string.Empty;
                if (next != null) next.IsActive = true;
            }
            Save();
        }
    }
}
