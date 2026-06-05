using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AvaloniaApplication1.Views.Settings;

public partial class SettingsOverviewView : UserControl
{
    public event Action? FrpcSourceNavigationRequested;

    public SettingsOverviewView()
    {
        InitializeComponent();
    }

    private void OnFrpcSourceClick(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine($">>> OnFrpcSourceClick | event null? {FrpcSourceNavigationRequested == null}");

        FrpcSourceNavigationRequested?.Invoke();

        // 保底：如果事件订阅没成功，直接找父 SettingsView 调用
        if (FrpcSourceNavigationRequested == null)
        {
            Console.WriteLine(">>> OnFrpcSourceClick: event was null, trying direct parent navigation");
            var settingsView = this.FindAncestorOfType<SettingsView>();
            Console.WriteLine($">>> OnFrpcSourceClick: settingsView found? {settingsView != null}");
            settingsView?.NavigateToFrpcReleaseSource();
        }
    }
}
