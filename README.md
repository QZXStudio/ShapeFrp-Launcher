# ShapeFrp Launcher

FRP 内网穿透隧道启动器，支持管理 ShapeFrp 隧道和自定义隧道的生命周期和记录活动日志。

## Project Structure

```
ShapeFrp-Launcher/
├── ShapeFrpLauncher/          # Main application (Avalonia + .NET 8)
│   ├── Program.cs
│   ├── App.axaml / .cs
│   ├── MainWindow.axaml / .cs
│   ├── ViewModels/
│   │   ├── ViewModelBase.cs
│   │   ├── HomeViewModel.cs
│   │   ├── CreateTunnelViewModel.cs
│   │   ├── SettingsViewModel.cs
│   │   └── AboutViewModel.cs
│   └── Views/
│       ├── HomeView.axaml / .cs
│       ├── CreateTunnelView.axaml / .cs
│       ├── FrpCoreView.axaml / .cs
│       ├── SettingsView.axaml / .cs
│       ├── AboutView.axaml / .cs
│       └── Card.axaml / .cs
├── QZXFrp.Setup/                  # WiX MSI installer
│   ├── QZXFrp.Setup.wixproj
│   ├── Package.wxs
│   └── License.rtf
├── QZXFrp.Bootstrapper/           # WiX Burn EXE bootstrapper
│   ├── QZXFrp.Bootstrapper.wixproj
│   └── Bundle.wxs
├── build.ps1                      # Windows build script
├── build.sh                       # Linux/macOS build script
└── QZXFrp.slnx                    # Solution file
```

## Build

```powershell
# Windows (MSI + EXE)
.\build.ps1

# Linux (.deb + .rpm)
./build.sh linux-x64

# macOS (.dmg)
./build.sh osx-arm64
```
