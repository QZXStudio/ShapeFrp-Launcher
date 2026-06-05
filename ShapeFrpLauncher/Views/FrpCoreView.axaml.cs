using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using AvaloniaApplication1.ViewModels;
using AvaloniaApplication1.Views.FrpCore;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;

namespace AvaloniaApplication1.Views;

public partial class FrpCoreView : UserControl
{
    private static readonly SlideNavigationTransitionInfo ForwardTransition = new()
    {
        Effect = SlideNavigationTransitionEffect.FromRight
    };
    private static readonly SlideNavigationTransitionInfo BackTransition = new()
    {
        Effect = SlideNavigationTransitionEffect.FromLeft
    };

    private readonly ObservableCollection<object> _breadcrumbItems = new();
    private ReleaseListViewModel? _releaseListVM;

    public bool CanGoBack => ContentFrame.CanGoBack;
    public event Action? CanGoBackChanged;

    public FrpCoreView()
    {
        InitializeComponent();
        DataContext = new FrpCoreViewModel();

        Breadcrumb.ItemsSource = _breadcrumbItems;
        Breadcrumb.ItemClicked += OnBreadcrumbItemClicked;

        ContentFrame.Navigated += OnContentFrameNavigated;

        SetBreadcrumbSingle("Frp 核心");
        ContentFrame.Navigate(typeof(FrpCoreOverviewView));
    }

    private void OnContentFrameNavigated(object? sender, NavigationEventArgs e)
    {
        if (e.Content is ReleaseListView)
        {
            SetBreadcrumbTwo("Frp 核心", "下载Frpc核心");
            PageDescription.Text = "从远程仓库获取 Frpc 的 Release 信息";
        }
        else
        {
            SetBreadcrumbSingle("Frp 核心");
            PageDescription.Text = "管理和创建 Frpc 版本";

            if (e.Content is FrpCoreOverviewView overview)
            {
                overview.DownloadFrpcRequested -= OnDownloadFrpcRequested;
                overview.DownloadFrpcRequested += OnDownloadFrpcRequested;
            }
        }

        CanGoBackChanged?.Invoke();
    }

    private void OnBreadcrumbItemClicked(object? sender, BreadcrumbBarItemClickedEventArgs e)
    {
        if (e.Index == 0 && ContentFrame.CanGoBack)
            ContentFrame.GoBack(BackTransition);
    }

    private void OnDownloadFrpcRequested()
    {
        _releaseListVM ??= new ReleaseListViewModel();
        ReleaseListView.SetSharedViewModel(_releaseListVM);
        ContentFrame.Navigate(typeof(ReleaseListView), null, ForwardTransition);
    }

    public bool HandleBackNavigation()
    {
        if (ContentFrame.CanGoBack)
        {
            ContentFrame.GoBack(BackTransition);
            return true;
        }
        return false;
    }

    private void SetBreadcrumbSingle(string label)
    {
        _breadcrumbItems.Clear();
        _breadcrumbItems.Add(label);
    }

    private void SetBreadcrumbTwo(string parent, string current)
    {
        _breadcrumbItems.Clear();
        _breadcrumbItems.Add(parent);
        _breadcrumbItems.Add(current);
    }
}
