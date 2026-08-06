<div align="center">
  <img src="https://via.placeholder.com/200/1a1a1a/ffffff?text=Pierre+Client" alt="Pierre Client Logo" width="200" />
  <h1>Pierre Client</h1>
  <p><strong>A custom-built, ultra-optimized Minecraft 1.21.11 client and launcher.</strong></p>

  [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
  [![Platform](https://img.shields.io/badge/Platform-Windows-blue.svg)](#)
  [![Minecraft](https://img.shields.io/badge/Minecraft-1.21.11-success.svg)](#)
</div>

<br/>

## 🚀 Features

Pierre Client is an advanced Minecraft launcher and client integration written from scratch in C# and .NET Core.

* **Minecraft 1.21.11 Support:** Fully supports and launches the custom 1.21.11 version.
* **Auto-Installer:** Automatically downloads and synchronizes required Fabric libraries and mods directly into your game folder.
* **Smart Java Management:** Uses CmlLib to automatically locate or download the correct Java 21 runtime.
* **Native Discord RPC:** Built-in Discord Rich Presence displays your game status directly from the launcher without needing in-game mods that cause version conflicts.
* **Command-Line Limit Bypass:** Uses Java `@argfile` injection to bypass the Windows 8191 character limit, preventing silent crashes when launching heavily modded instances.
* **Sleek UI:** Modern and lightweight WPF-based user interface.

## 📥 Download & Install

1. Download the latest source from the repository.
2. Ensure you have the [.NET 10.0 SDK](https://dotnet.microsoft.com/download) installed on your system.
3. Build the project and run the output executable.

## 🛠️ How to Compile

To compile the launcher from source, open a terminal in the project directory and run:

```bash
cd PierreLauncher
dotnet build -c Release
```

The compiled executable will be located in `PierreLauncher/bin/Release/net10.0-windows/`.
Alternatively, you can run `Start.bat` (formerly Oyunu Başlat.bat) from the root directory for quick testing.

## ⚙️ Architecture

- **Framework:** .NET 10.0 (WPF)
- **Core Library:** CmlLib.Core for Minecraft environment management.
- **RPC:** DiscordRichPresence library for C#.

## 📜 License

This project is open-source and available under the MIT License.
