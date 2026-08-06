<div align="center">
  <img src="https://via.placeholder.com/150/1a1a1a/ffffff?text=Pierre+Client" alt="Pierre Client Logo" width="150" />
  <h1>Pierre Client 1.21.11</h1>
  <p><strong>Minecraft 1.21.11 için Özel Olarak Geliştirilmiş, Ultra Optimize Fabric İstemcisi</strong></p>
</div>

<br/>

## 🚀 Özellikler

Pierre Client, standart Minecraft'ın ötesine geçmek için C# ve .NET Core kullanılarak sıfırdan yazılmış bir Launcher sistemine sahiptir.

* **Özel 1.21.11 Versiyonu:** Minecraft'ın özel yapım 1.21.11 sürümünü tam uyumlulukla çalıştırır.
* **Otomatik Mod ve Kütüphane Kurulumu:** Gerekli tüm Fabric kütüphanelerini, performans modlarını ve RPC eklentilerini oyun dizinine otomatik kurar.
* **Akıllı Java Yönetimi:** CmlLib destekli sistem sayesinde en uygun Java 21 sürümünü bulur veya indirir.
* **Discord Rich Presence (RPC):** Arka planda C# üzerinden Discord hesabınıza "Minecraft Oynuyor - Pierre Client" durumunu yansıtır. Mod çakışmalarını (SimpleRPC) ortadan kaldırır.
* **Komut Satırı Sınırı Aşımı (Bypass):** Windows'un 8191 karakterlik devasa mod komut satırı çökertme sorununu Java `@argfile` teknolojisiyle ezer geçer. Sessiz çökmelere son!
* **Hafif ve Hızlı:** WPF tabanlı modern ve akıcı arayüz.

## 🛠️ Kurulum ve Kullanım

1. **İndirin:** Bu depoyu sağ üstten `Code > Download ZIP` diyerek veya `git clone` ile bilgisayarınıza indirin.
2. **Derleyin (Geliştiriciler İçin):** `PierreLauncher` klasörüne girip `dotnet build` komutunu çalıştırarak `.exe` dosyanızı oluşturabilirsiniz.
3. **Çalıştırın:** Çıktı klasöründeki `PierreLauncher.exe` dosyasını çalıştırın veya ana dizindeki `Oyunu Başlat.bat` dosyasını kullanın.
4. Launcher açıldığında doğrudan **"Oyna"** butonuna basın. Gerisini Pierre halleder!

## 🔧 Teknik Detaylar

- **Framework:** .NET 10.0 (Windows)
- **UI Kütüphanesi:** WPF (Windows Presentation Foundation)
- **Minecraft Core:** CmlLib.Core (v4.0.0-beta)
- **Discord Entegrasyonu:** DiscordRichPresence (v1.2.1.24)

## 🐛 Bilinen Sorunlar ve Çözümleri
* *CraterLib uyuşmazlığı:* Modrinth üzerindeki `craterlib` (3.x) sürümü 1.21.11 yapısı ile uyuşmazlık gösterdiği için yükleme listesinden kaldırılmış, tüm entegrasyonlar ana Launcher içine C# ile gömülmüştür. Mod içine atmayın.
* *Sessiz Kapanma (Log Yok):* Java `@argfile` sayesinde tamamen çözüldü.

---
<div align="center">
  Geliştirici: <b>Pierre Ekibi</b>
</div>
