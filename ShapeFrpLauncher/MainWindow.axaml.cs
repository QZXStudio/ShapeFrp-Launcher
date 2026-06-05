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
    private bool _isNavigating = false;
    private Views.FrpCoreView? _frpCoreView;
    private Views.SettingsView? _settingsView;

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
        if (_isNavigating) return;

        if (e.SelectedItem is not NavigationViewItem { Tag: string tag })
            return;
        if (!PageTypes.TryGetValue(tag, out var pageType))
            return;

        var currentType = ContentFrame.Content?.GetType();
        if (currentType == pageType) return;

        // 离开当前页面时取消内部导航订阅
        UnsubscribeFrpCore();
        UnsubscribeSettings();

        _isNavigating = true;

        try
        {
            if (currentType != null && currentType != pageType)
                _backStack.Push(currentType);

            ContentFrame.Navigate(pageType);

            // 进入新页面时订阅内部导航事件
            SubscribeFrpCore();
            SubscribeSettings();

            UpdateBackButtonState();
        }
        finally
        {
            _isNavigating = false;
        }
    }

    private void BackButton_Click(object? sender, RoutedEventArgs e)
    {
        // 优先让当前页面处理内部返回（子页面 → 概览）
        if (_frpCoreView is { CanGoBack: true })
        {
            _frpCoreView.HandleBackNavigation();
            return;
        }
        if (_settingsView is { CanGoBack: true })
        {
            _settingsView.HandleBackNavigation();
            return;
        }

        if (_backStack.Count == 0) return;

        var prevType = _backStack.Pop();

        _isNavigating = true;

        UnsubscribeFrpCore();
        UnsubscribeSettings();

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

            SubscribeFrpCore();
            SubscribeSettings();
            UpdateBackButtonState();
        }
        finally
        {
            _isNavigating = false;
        }
    }

    private void SubscribeFrpCore()
    {
        if (ContentFrame.Content is Views.FrpCoreView frpCore)
        {
            _frpCoreView = frpCore;
            _frpCoreView.CanGoBackChanged += UpdateBackButtonState;
        }
    }

    private void UnsubscribeFrpCore()
    {
        if (_frpCoreView != null)
        {
            _frpCoreView.CanGoBackChanged -= UpdateBackButtonState;
            _frpCoreView = null;
        }
    }

    private void SubscribeSettings()
    {
        if (ContentFrame.Content is Views.SettingsView settings)
        {
            _settingsView = settings;
            _settingsView.CanGoBackChanged += UpdateBackButtonState;
        }
    }

    private void UnsubscribeSettings()
    {
        if (_settingsView != null)
        {
            _settingsView.CanGoBackChanged -= UpdateBackButtonState;
            _settingsView = null;
        }
    }

    private void UpdateBackButtonState()
    {
        BackButton.IsEnabled = _backStack.Count > 0
            || (_frpCoreView?.CanGoBack ?? false)
            || (_settingsView?.CanGoBack ?? false);
    }

    private void LoginItem_Tapped(object? sender, RoutedEventArgs e)
    {
        // 登录/注册 — 后续实现
    }
}
