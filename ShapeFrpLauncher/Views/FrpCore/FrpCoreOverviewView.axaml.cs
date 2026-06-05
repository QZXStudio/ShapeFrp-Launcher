using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaApplication1.ViewModels;
using FluentAvalonia.UI.Controls;
using System;

namespace AvaloniaApplication1.Views.FrpCore;

public partial class FrpCoreOverviewView : UserControl
{
    public event Action? DownloadFrpcRequested;

    public FrpCoreOverviewView()
    {
        InitializeComponent();
        DataContext = new FrpCoreViewModel();
    }

    private async void DownloadFrpc_Click(object? sender, RoutedEventArgs e)
    {
      


        DownloadFrpcRequested?.Invoke();
    }
}
