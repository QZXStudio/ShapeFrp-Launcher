using Avalonia.Controls;
using AvaloniaApplication1.ViewModels;

namespace AvaloniaApplication1.Views;

public partial class AboutView : UserControl
{
    public AboutView()
    {
        InitializeComponent();
        DataContext = new AboutViewModel();
    }
}
