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

        // 每次加载都检查源是否变更，若变更则全量刷新
        _ = _vm.EnsureFreshAsync();
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
    /// </summary>
    private void OnCardLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control card) return;
        if (card.Tag is not null) return;
        card.Tag = true;

        card.Opacity = 0;
        var slide = new TranslateTransform(20, 0);
        card.RenderTransform = slide;

        Dispatcher.UIThread.Post(() =>
        {
            card.Transitions = new Transitions
            {
                new DoubleTransition
                {
                    Property = Control.OpacityProperty,
                    Duration = TimeSpan.FromSeconds(0.35),
                },
            };
            slide.Transitions = new Transitions
            {
                new DoubleTransition
                {
                    Property = TranslateTransform.XProperty,
                    Duration = TimeSpan.FromSeconds(0.35),
                },
            };

            card.Opacity = 1;
            slide.X = 0;
        });
    }
}
