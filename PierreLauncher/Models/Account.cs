using System;

namespace PierreLauncher.Models
{
    public class Account
    {
        public string Username { get; set; } = string.Empty;
        public string AccountType { get; set; } = "Çevrimdışı"; // "PierreClient", "Microsoft", "Çevrimdışı"
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastLoginTime { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = false;

        public string DisplayLastLogin => 
            (DateTime.Now - LastLoginTime).TotalMinutes < 2 
                ? "Son giriş: a few seconds ago" 
                : $"Son giriş: {(int)(DateTime.Now - LastLoginTime).TotalMinutes} dakika önce";
    }
}
