﻿

# Raymond Maarloeve Launcher

> ⚠ This is the official **launcher** for the [RaymondMaarloeve game project](https://github.com/RaymondMaarloeve/RaymondMaarloeve).

![.NET](https://img.shields.io/badge/.NET-9.0-blueviolet)
![UI](https://img.shields.io/badge/GUI-AvaloniaUI-green)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux-informational)
![Last Commit](https://img.shields.io/github/last-commit/RaymondMaarloeve/RaymondMaarloeveLauncher)


A modern, cross-platform launcher for the **Raymond Maarloeve** game project, built with [Avalonia UI](https://avaloniaui.net/).

It provides a user-friendly interface to download, manage and launch game builds directly from GitHub Releases.

<p align="center">
  <img src="https://github.com/cyptrix12/RaymondMaarloeveLauncher/blob/master/Preview.png" alt="Launcher Preview" width="600"/>
</p>

## ✨ Features

- 🔍 Fetches list of GitHub releases dynamically
- 🧠 Downloads and extracts selected game builds (ZIP), LLM server and LLM models
- 💬 Displays detailed changelog from latest release (Markdown-rendered)
- 🛠 Automatically tracks and displays the current installed game version
- 🚀 Launches the game directly from the launcher
- 🌗 Supports both dark and light modes (system-aware color palette)

## 🧩 Technologies

- [.NET 9](https://dotnet.microsoft.com/)
- [Avalonia UI](https://avaloniaui.net/) – cross-platform GUI
- [Octokit](https://github.com/octokit/octokit.net) – GitHub API client
- [Markdown.Avalonia.Tight](https://github.com/whistyun/Markdown.Avalonia) – Markdown rendering


## 🚀 Usage

[⬇️ Download latest build](https://github.com/RaymondMaarloeve/RaymondMaarloeveLauncher/releases/latest)

1. Launch the app (`RaymondMaarloeveLauncher.exe`)
2. Choose and download releases (game, server)
3. Choose and download LLM models
4. Config NPC's and game settings
6. Click **Launch game** – the `.exe` from the build will be run
7. Done!

## 🛠 Build instructions

To build locally:

```bash
git clone https://github.com/cyptrix12/RaymondMaarloeveLauncher.git
cd RaymondMaarloeveLauncher
dotnet restore
dotnet build -c Release
```

To publish:

```bash
dotnet publish -c Release -r win-x64 --self-contained true
```

---

> This launcher is part of a larger project centered around the world of Raymond Maarloeve.  
> Stay tuned for game updates, models, and more.
