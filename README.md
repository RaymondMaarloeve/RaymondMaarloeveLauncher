

# Raymond Maarloeve Launcher

> ⚠ This is the official **launcher** for the [RaymondMaarloeve game project](https://github.com/Gitmanik/RaymondMaarloeve).

A modern, cross-platform launcher for the **Raymond Maarloeve** game project, built with [Avalonia UI](https://avaloniaui.net/).

<p align="center">
  <img src="https://github.com/cyptrix12/RaymondMaarloeveLauncher/blob/master/Preview.png" alt="Launcher Preview" width="600"/>
</p>

## ✨ Features

- 🔍 Fetches list of GitHub releases dynamically
- 🧠 Downloads and extracts selected game builds (ZIP)
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

1. Launch the app (`RaymondMaarloeveLauncher.exe`)
2. Choose and download a release
3. View changelog and installed version
4. Click **Launch game** – the `.exe` from the build will be run
5. Done!

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
