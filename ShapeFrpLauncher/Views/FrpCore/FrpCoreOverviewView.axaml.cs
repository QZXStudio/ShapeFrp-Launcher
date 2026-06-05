using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaApplication1.ViewModels;

namespace AvaloniaApplication1.Views.FrpCore;

public partial class FrpCoreOverviewView : UserControl
{
    public event Action? DownloadFrpcRequested;

    public FrpCoreOverviewView()
    {
        InitializeComponent();
        DataContext = new FrpCoreViewModel();
    }

    private void DownloadFrpc_Click(object? sender, RoutedEventArgs e)
    {
        DownloadFrpcRequested?.Invoke();
    }
}
