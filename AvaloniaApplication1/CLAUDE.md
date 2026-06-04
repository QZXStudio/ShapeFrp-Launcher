# QZXFrp — Avalonia 桌面应用项目规范

## 技术栈
- .NET 8.0，Avalonia **11.3.5**，FluentAvaloniaUI **2.4.1**，Fluent 主题，Inter 字体
- 编译绑定默认开启 (`AvaloniaUseCompiledBindingsByDefault=true`)
- 导航：FluentAvaloniaUI `NavigationView`（自带 Win11 风格侧边栏和选中动画）
- MVVM 模式：每个 View 在构造函数中设置自己的 `DataContext`

## 窗口架构
- `MainWindow` 继承 `FluentAvalonia.UI.Windowing.AppWindow`（非原生 `Window`），用于沉浸式标题栏 + 拖拽支持
- 构造函数中配置：
  - `TitleBar.ExtendsContentIntoTitleBar = true` — 内容延展到标题栏区域
  - `TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex` — 自动区分拖拽和控件交互
  - `TitleBar.Height = 40` — 标题栏高度
- 根布局 `<Grid RowDefinitions="40,*">`：
  - Row 0: `<Border Background="{DynamicResource SolidBackgroundFillColorBaseBrush}">` — 与内容区同色的 40px 顶栏，作为可拖拽标题栏区域
  - Row 1: `<ui:NavigationView>` — 填充剩余空间
- **不要**回退到原生 `Window` + 手动 `ExtendClientArea*` 属性——拖拽会失效
- **不要**去掉顶栏或改成透明 overlay——顶栏提供全局拖拽区域和返回按钮的承载

## 导航架构
- `NavigationView`（`PaneDisplayMode="Left"`），4 个 `NavigationViewItem`：主页、创建隧道、设置、关于
- `MainWindow.axaml.cs` 通过 `SelectionChanged` 事件用 `Dictionary<string, Type>` 切换 `ContentFrame`
- 返回栈 `Stack<Type> _backStack` 管理后退导航
- 全局返回按钮位于顶栏最左侧（`Margin="12,0,0,0"`, `VerticalAlignment="Center"`），不在页面内部
- **不需要** `MainViewModel` / `NavItem`——NavigationView 自己处理选中状态和视觉样式

## 编译绑定要求
- 包含 `{Binding}` 的 XAML 元素必须有 `x:DataType` 声明
- NavigationView 内部目前没有自定义 Binding，所以不涉及 DataType

## 主题
- `App.axaml.cs` 中 `Styles.Add(new FluentAvaloniaTheme())` 载入 FluentAvalonia 主题
- `App.axaml` 保留 `<FluentTheme />` 作为基础

## 项目文件结构
```
AvaloniaApplication1/
  Program.cs              — 入口，STAThread
  App.axaml / .cs         — Application + FluentAvaloniaTheme
  MainWindow.axaml / .cs  — AppWindow + 顶栏（返回按钮）+ NavigationView
  ViewModels/
    ViewModelBase.cs      — INotifyPropertyChanged 基类
    HomeViewModel.cs
    CreateTunnelViewModel.cs
    SettingsViewModel.cs
    AboutViewModel.cs
  Views/
    HomeView.axaml / .cs
    CreateTunnelView.axaml / .cs
    SettingsView.axaml / .cs
    AboutView.axaml / .cs
    Card.axaml / .cs      — 通用卡片组件（Title / Desc StyledProperty）
```

## 打包与分发

### Windows MSI（WiX NuGet SDK，零外部依赖）

```
项目根目录/
├── QZXFrp.Setup/
│   ├── QZXFrp.Setup.wixproj  ← WiX SDK (NuGet: WixToolset.Sdk + UI.wixext v6)
│   ├── Package.wxs           ← MSI 定义：特性树、目录、快捷方式、自动采集发布文件
│   └── License.rtf           ← 安装许可
├── build.ps1                 ← dotnet build QZXFrp.Setup -c Release
├── build.sh                  ← Linux/macOS (.deb/.rpm/.dmg)
└── QZXFrp.slnx               ← 解决方案含 Setup 项目
```

**在 Visual Studio 中直接构建**（无需装任何外部工具）：
- Build 解决方案 → NuGet 自动还原 WiX SDK → 自动 publish 主项目 → 打包 MSI
- 输出: `QZXFrp.Setup/bin/Release/QZXFrp.Setup.msi`

**命令行**:
```powershell
dotnet build QZXFrp.Setup/QZXFrp.Setup.wixproj -c Release
# 或: .\build.ps1
```

**MSI 安装选项**（WixUI_FeatureTree 组件选择对话框）:
- ☑ QZXFrp 主程序（必装，不可取消）
  - ☐ 桌面快捷方式（默认不选）
- ☑ FRP 核心 frpc.exe（可选，默认勾选）

**版本号**:
- 默认 `yy.M.d` 格式（MSI 兼容：major<256, minor<256）
- 覆盖: `dotnet build -p:Version=1.2.3`

**扩展安装组件**: 编辑 `Package.wxs` 的 `<Feature>` 树 → 添加 `<Feature Id="X" Title="..." Level="1">`

### Linux / macOS

```bash
./build.sh linux-x64      # dist/qzxfrp_<ver>_amd64.deb + .rpm
./build.sh osx-arm64      # dist/QZXFrp_<ver>_arm64.dmg
```
依赖: `dpkg-deb`（系统自带）, `rpmbuild`（可选），macOS 需 `hdiutil`（系统自带）

## 禁止的做法
- 不要在 Avalonia 12.x 下开发（项目已锁定 11.3.5）
- 不要用自定义 ListBox/pill 模拟导航——用 FluentAvaloniaUI NavigationView
- 不要在 XAML 中嵌套复杂的动画声明（Easing/DoubleTransition 等），在代码中设置
- 不要添加 AvaloniaUI.DiagnosticsSupport
