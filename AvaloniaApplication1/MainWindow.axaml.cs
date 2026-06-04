using System;
using System.Collections.Generic;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;

namespace AvaloniaApplication1;

public partial class MainWindow : AppWindow
{
    private static readonly Dictionary<string, Type> PageTypes = new()
    {
        ["Home"] = typeof(Views.HomeView),
        ["CreateTunnel"] = typeof(Views.CreateTunnelView),
        ["FrpCore"] = typeof(Views.FrpCoreView),
        ["Settings"] = typeof(Views.SettingsView),
        ["About"] = typeof(Views.AboutView),
    };

    private readonly Stack<Type> _backStack = new();
    private bool _isNavigating = false;  // 防重入标志

    public MainWindow()
    {
        InitializeComponent();

        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;
        TitleBar.Height = 40;

        var first = (NavigationViewItem)NavView.MenuItems[0]!;
        NavView.SelectedItem = first;
        ContentFrame.Navigate(typeof(Views.HomeView));
    }

    private void NavView_SelectionChanged(object sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (_isNavigating) return;  // 防止重入

        if (e.SelectedItem is not NavigationViewItem { Tag: string tag })
            return;
        if (!PageTypes.TryGetValue(tag, out var pageType))
            return;

        var currentType = ContentFrame.Content?.GetType();
        if (currentType == pageType) return;  // 相同页面不重复导航

        _isNavigating = true;

        try
        {
            if (currentType != null && currentType != pageType)
                _backStack.Push(currentType);

            ContentFrame.Navigate(pageType);
            BackButton.IsEnabled = _backStack.Count > 0;
        }
        finally
        {
            _isNavigating = false;
        }
    }

    private void BackButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_backStack.Count == 0) return;

        var prevType = _backStack.Pop();

        _isNavigating = true;

        try
        {
            NavView.SelectionChanged -= NavView_SelectionChanged;
            ContentFrame.Navigate(prevType);
            NavView.SelectionChanged += NavView_SelectionChanged;

            foreach (NavigationViewItem item in NavView.MenuItems)
            {
                if (item.Tag is string tag && PageTypes.TryGetValue(tag, out var pt) && pt == prevType)
                {
                    NavView.SelectedItem = item;
                    break;
                }
            }

            BackButton.IsEnabled = _backStack.Count > 0;
        }
        finally
        {
            _isNavigating = false;
        }
    }

    private void LoginItem_Tapped(object? sender, RoutedEventArgs e)
    {
        // 登录/注册 — 后续实现
    }
}