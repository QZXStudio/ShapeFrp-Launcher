using Avalonia;
using Avalonia.Controls;

namespace AvaloniaApplication1.Views;

public partial class Card : UserControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<Card, string>(nameof(Title), string.Empty);

    public static readonly StyledProperty<string> DescProperty =
        AvaloniaProperty.Register<Card, string>(nameof(Desc), string.Empty);

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Desc
    {
        get => GetValue(DescProperty);
        set => SetValue(DescProperty, value);
    }

    public Card()
    {
        InitializeComponent();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TitleProperty)
            TitleBlock.Text = change.NewValue as string;
        else if (change.Property == DescProperty)
            DescBlock.Text = change.NewValue as string;
    }
}
