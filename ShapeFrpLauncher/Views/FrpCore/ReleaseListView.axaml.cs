using Avalonia.Controls;
using Avalonia.Controls.Primitives;
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

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
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
}
