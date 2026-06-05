using System;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaApplication1.ViewModels;

namespace AvaloniaApplication1.Views.FrpCore;

public partial class ReleaseListView : UserControl
{
    private static ReleaseListViewModel? s_pendingVM;

    private readonly ReleaseListViewModel _vm;
    private bool _initialized;

    public ReleaseListView()
    {
        InitializeComponent();

        _vm = s_pendingVM ?? new ReleaseListViewModel();
        s_pendingVM = null;
        DataContext = _vm;

        MainScrollViewer.ScrollChanged += OnScrollChanged;
        Loaded += OnLoaded;
    }

    public static void SetSharedViewModel(ReleaseListViewModel vm)
    {
        s_pendingVM = vm;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (_initialized) return;
        _initialized = true;

        if (_vm.Releases.Count > 0)
            _ = _vm.RefreshAsync();
        else
            _ = _vm.LoadFirstPageAsync();
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (_vm.IsLoading || !_vm.HasMore)
            return;

        var scroll = MainScrollViewer;
        var offset = scroll.Offset.Y;
        var extent = scroll.Extent.Height;
        var viewport = scroll.Viewport.Height;

        if (extent - offset - viewport < 200)
            _ = _vm.LoadNextPageAsync();
    }

    /// <summary>
    /// 卡片加载后的滑入+淡入动画，风格对齐 Frame 导航过渡。
    /// 先设初始态（透明 + 向右偏移），下一帧附着 Transition 再设终态，
    /// 触发 Opacity 0→1 和 Translate X 20→0 的双重过渡。
    /// </summary>
    private void OnCardLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not Border border) return;
        // 只执行一次（ItemsControl 复用容器时防止重复动画）
        if (border.Tag is not null) return;
        border.Tag = true;

        // 初始态：透明 + 从右滑入
        border.Opacity = 0;
        var slide = new TranslateTransform(20, 0);
        border.RenderTransform = slide;

        Dispatcher.UIThread.Post(() =>
        {
            // Opacity 淡入
            border.Transitions = new Transitions
            {
                new DoubleTransition
                {
                    Property = Border.OpacityProperty,
                    Duration = TimeSpan.FromSeconds(0.35),
                },
            };
            // Translate X 滑入（在 TranslateTransform 自身挂载 Transition）
            slide.Transitions = new Transitions
            {
                new DoubleTransition
                {
                    Property = TranslateTransform.XProperty,
                    Duration = TimeSpan.FromSeconds(0.35),
                },
            };

            // 同时触发
            border.Opacity = 1;
            slide.X = 0;
        });
    }
}
