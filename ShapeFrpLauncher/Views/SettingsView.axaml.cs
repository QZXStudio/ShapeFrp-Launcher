using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using AvaloniaApplication1.ViewModels;
using AvaloniaApplication1.Views.Settings;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;

namespace AvaloniaApplication1.Views;

public partial class SettingsView : UserControl
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

    public bool CanGoBack => ContentFrame.CanGoBack;
    public event Action? CanGoBackChanged;

    public SettingsView()
    {
        InitializeComponent();
        DataContext = new SettingsViewModel();

        Breadcrumb.ItemsSource = _breadcrumbItems;
        Breadcrumb.ItemClicked += OnBreadcrumbItemClicked;
        ContentFrame.Navigated += OnContentFrameNavigated;

        SetBreadcrumbSingle("设置");
        ContentFrame.Navigate(typeof(SettingsOverviewView));

        this.Loaded += OnLoaded;
    }

    // 公共导航方法，供子控件直接调用
    public void NavigateToFrpcReleaseSource()
    {
        Console.WriteLine($">>> SettingsView.NavigateToFrpcReleaseSource called");
        Console.WriteLine($">>> ContentFrame.Content before: {ContentFrame.Content?.GetType()?.Name}");
        ContentFrame.Navigate(typeof(FrpcReleaseSourceView), null, ForwardTransition);
        Console.WriteLine($">>> ContentFrame.Content after: {ContentFrame.Content?.GetType()?.Name}");
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        Console.WriteLine($">>> SettingsView.OnLoaded | Content={ContentFrame.Content?.GetType()?.Name}");
        SubscribeOverviewEvent();
    }

    private void SubscribeOverviewEvent()
    {
        if (ContentFrame.Content is SettingsOverviewView overview)
        {
            overview.FrpcSourceNavigationRequested -= OnFrpcSourceNavigationRequested;
            overview.FrpcSourceNavigationRequested += OnFrpcSourceNavigationRequested;
            Console.WriteLine($">>> SubscribeOverviewEvent: event subscribed OK");
        }
        else
        {
            Console.WriteLine($">>> SubscribeOverviewEvent: Content is {ContentFrame.Content?.GetType()?.Name ?? "null"}, NOT SettingsOverviewView");
        }
    }

    private void OnContentFrameNavigated(object? sender, NavigationEventArgs e)
    {
        Console.WriteLine($">>> SettingsView.OnContentFrameNavigated | content={e.Content?.GetType()?.Name}");

        if (e.Content is FrpcReleaseSourceView)
        {
            SetBreadcrumbTwo("设置", "Frpc Release 获取来源");
            PageDescription.Text = "配置 Release 的远程仓库来源平台";
        }
        else
        {
            SetBreadcrumbSingle("设置");
            PageDescription.Text = "配置 QZXFrp 客户端的行为和偏好";

            if (e.Content is SettingsOverviewView overview)
            {
                overview.FrpcSourceNavigationRequested -= OnFrpcSourceNavigationRequested;
                overview.FrpcSourceNavigationRequested += OnFrpcSourceNavigationRequested;
                Console.WriteLine($">>> Subscribed to FrpcSourceNavigationRequested");
            }
        }

        CanGoBackChanged?.Invoke();
    }

    private void OnBreadcrumbItemClicked(object? sender, BreadcrumbBarItemClickedEventArgs e)
    {
        if (e.Index == 0 && ContentFrame.CanGoBack)
            ContentFrame.GoBack(BackTransition);
    }

    private void OnFrpcSourceNavigationRequested()
    {
        Console.WriteLine($">>> SettingsView.OnFrpcSourceNavigationRequested called");
        Console.WriteLine($">>> ContentFrame.Content before: {ContentFrame.Content?.GetType()?.Name}");
        ContentFrame.Navigate(typeof(FrpcReleaseSourceView), null, ForwardTransition);
        Console.WriteLine($">>> ContentFrame.Content after: {ContentFrame.Content?.GetType()?.Name}");
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
