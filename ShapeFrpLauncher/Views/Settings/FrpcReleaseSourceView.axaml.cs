using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaApplication1.Services;
using FluentAvalonia.UI.Controls;

namespace AvaloniaApplication1.Views.Settings;

public partial class FrpcReleaseSourceView : UserControl
{
    public FrpcReleaseSourceView()
    {
        InitializeComponent();

        SourceComboBox.ItemsSource = new[]
        {
            "GitHub",
            "Gitee",
            "GitLab"
        };
        SourceComboBox.SelectedIndex = (int)ReleaseSourceConfig.CurrentSource;
        SourceComboBox.SelectionChanged += OnSourceChanged;

        TokenBox.Text = ReleaseSourceConfig.GitHubToken ?? "";
        TokenBox.TextChanged += OnTokenTextChanged;

        UpdateButtonMode();
    }

    private void UpdateButtonMode()
    {
        var hasText = !string.IsNullOrWhiteSpace(TokenBox.Text);
        TokenActionButton.IsVisible = hasText;
        TokenHelpButton.IsVisible = !hasText;
    }

    private void OnSourceChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (SourceComboBox.SelectedIndex < 0) return;
        ReleaseSourceConfig.CurrentSource = (ReleaseSourceType)SourceComboBox.SelectedIndex;
        ReleaseSourceConfig.Save();
    }

    private void OnTokenTextChanged(object? sender, TextChangedEventArgs e)
    {
        var token = TokenBox.Text?.Trim();
        ReleaseSourceConfig.GitHubToken = string.IsNullOrEmpty(token) ? null : token;
        ReleaseSourceConfig.Save();
        UpdateButtonMode();
    }

    private async void OnTokenTestClick(object? sender, RoutedEventArgs e)
    {
        var token = TokenBox.Text?.Trim();
        if (string.IsNullOrEmpty(token)) return;

        TokenActionButton.IsVisible = false;
        TokenTestSpinner.IsVisible = true;
        TokenTestSpinner.IsActive = true;

        try
        {
            var (ok, message) = await GitHubReleaseService.VerifyTokenAsync(token);

            var dialog = new ContentDialog
            {
                Title = ok ? "验证通过" : "验证失败",
                Content = message,
                CloseButtonText = "确定",
                DefaultButton = ContentDialogButton.Close,
            };

            await dialog.ShowAsync();

            if (ok)
            {
                ReleaseSourceConfig.GitHubToken = token;
                ReleaseSourceConfig.Save();
            }
        }
        finally
        {
            TokenTestSpinner.IsActive = false;
            TokenTestSpinner.IsVisible = false;
            TokenActionButton.IsVisible = true;
        }
    }

    private async void OnTokenHelpClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/settings/tokens/new?description=QZXFrp&scopes=repo",
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        }
        catch
        {
            // 跨平台兼容：某些环境不支持直接打开浏览器
        }
    }
}
