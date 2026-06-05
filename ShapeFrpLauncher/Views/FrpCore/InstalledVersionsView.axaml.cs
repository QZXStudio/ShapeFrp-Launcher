using Avalonia.Controls;
using AvaloniaApplication1.ViewModels;

namespace AvaloniaApplication1.Views.FrpCore;

public partial class InstalledVersionsView : UserControl
{
    public InstalledVersionsView()
    {
        InitializeComponent();
        DataContext = new InstalledVersionsViewModel();
    }
}
