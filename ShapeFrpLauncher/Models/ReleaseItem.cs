using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AvaloniaApplication1.Models;

public class ReleaseAssetInfo
{
    public string FileName { get; }
    public string DownloadUrl { get; }
    public long Size { get; }
    public string Platform { get; }      // "Windows", "Linux", "macOS", "FreeBSD"
    public string Architecture { get; }  // "amd64", "arm64", "arm", "386"
    public bool IsRecommended { get; }
    public string DisplaySize => Size switch
    {
        >= 1_048_576 => $"{Size / 1_048_576.0:F1} MB",
        >= 1_024 => $"{Size / 1_024.0:F1} KB",
        _ => $"{Size} B"
    };

    public ReleaseAssetInfo(string fileName, string downloadUrl, long size,
        string platform, string architecture, bool isRecommended)
    {
        FileName = fileName;
        DownloadUrl = downloadUrl;
        Size = size;
        Platform = platform;
        Architecture = architecture;
        IsRecommended = isRecommended;
    }
}

public class ReleaseItem : INotifyPropertyChanged
{
    public string TagName { get; }
    public string Name { get; }
    public DateTimeOffset PublishedAt { get; }
    public string Body { get; }
    public List<ReleaseAssetInfo> Assets { get; } = new();
    public string DisplayDate => PublishedAt.ToString("yyyy-MM-dd");

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

    public ReleaseItem(Octokit.Release release, IReadOnlyList<Octokit.ReleaseAsset> allAssets)
    {
        TagName = release.TagName;
        Name = release.Name ?? release.TagName;
        PublishedAt = release.PublishedAt ?? release.CreatedAt;
        Body = release.Body ?? "";

        Assets = allAssets
            .Select(ParseOctokitAsset)
            .Where(a => a is not null)
            .Cast<ReleaseAssetInfo>()
            .ToList();
    }

    /// <summary>供 Gitee / GitLab 等非 GitHub 源使用的构造器</summary>
    public ReleaseItem(string tagName, string name, DateTimeOffset publishedAt, string body,
        List<(string fileName, string downloadUrl, long size)> rawAssets)
    {
        TagName = tagName;
        Name = name;
        PublishedAt = publishedAt;
        Body = body;

        Assets = rawAssets
            .Select(a => ParseFromParts(a.fileName, a.downloadUrl, a.size))
            .Where(a => a is not null)
            .Cast<ReleaseAssetInfo>()
            .ToList();
    }

    private static ReleaseAssetInfo? ParseOctokitAsset(Octokit.ReleaseAsset asset)
        => ParseFromParts(asset.Name, asset.BrowserDownloadUrl, asset.Size);

    private static ReleaseAssetInfo? ParseFromParts(string name, string downloadUrl, long size)
    {
        // 只保留 zip / tar.gz 二进制包
        if (!name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) &&
            !name.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase))
            return null;
        if (name.Contains("checksums", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("sha256", StringComparison.OrdinalIgnoreCase))
            return null;

        // 源码归档（Gitee 等无二进制附件的源）
        if (name.Contains("source", StringComparison.OrdinalIgnoreCase))
        {
            return new ReleaseAssetInfo(name, downloadUrl, size, "Source", "source", false);
        }

        var baseName = name.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase)
            ? name[..^7] : name[..^4];

        var parts = baseName.Split('_');
        if (parts.Length < 4) return null;

        var os = parts[^2];
        var arch = parts[^1];

        var platform = os switch
        {
            "windows" => "Windows",
            "linux" => "Linux",
            "darwin" => "macOS",
            "freebsd" => "FreeBSD",
            _ => os
        };

        var isRecommended = os == "windows" && arch == "amd64";

        return new ReleaseAssetInfo(name, downloadUrl, size, platform, arch, isRecommended);
    }
}
