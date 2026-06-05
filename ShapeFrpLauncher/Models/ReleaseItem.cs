using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AvaloniaApplication1.Models;

public class ReleaseItem : INotifyPropertyChanged
{
    public string TagName { get; }
    public string Name { get; }
    public DateTimeOffset PublishedAt { get; }
    public string Body { get; }
    public string? AssetName { get; }
    public string? DownloadUrl { get; }
    public long? DownloadSize { get; }
    public string DisplayDate => PublishedAt.ToString("yyyy-MM-dd");
    public string DisplaySize => DownloadSize.HasValue
        ? DownloadSize.Value switch
        {
            >= 1_048_576 => $"{DownloadSize.Value / 1_048_576.0:F1} MB",
            >= 1_024 => $"{DownloadSize.Value / 1_024.0:F1} KB",
            _ => $"{DownloadSize.Value} B"
        }
        : "—";

    private bool _isExpanded;
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded == value) return;
            _isExpanded = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public ReleaseItem(Octokit.Release release, Octokit.ReleaseAsset? asset)
    {
        TagName = release.TagName;
        Name = release.Name ?? release.TagName;
        PublishedAt = release.PublishedAt ?? release.CreatedAt;
        Body = release.Body ?? "";
        AssetName = asset?.Name;
        DownloadUrl = asset?.BrowserDownloadUrl;
        DownloadSize = asset?.Size;
    }
}
