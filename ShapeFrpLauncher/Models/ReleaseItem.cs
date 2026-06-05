using System;

namespace AvaloniaApplication1.Models;

public class ReleaseItem
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

    public ReleaseItem(Octokit.Release release, Octokit.ReleaseAsset? asset)
    {
        TagName = release.TagName;
        Name = release.Name ?? release.TagName;
        PublishedAt = release.PublishedAt ?? release.CreatedAt;
        Body = TruncateBody(release.Body ?? "");
        AssetName = asset?.Name;
        DownloadUrl = asset?.BrowserDownloadUrl;
        DownloadSize = asset?.Size;
    }

    private static string TruncateBody(string body, int maxLen = 200)
    {
        var firstLine = body.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var text = firstLine.Length > 0 ? firstLine[0].Trim() : "";
        if (text.Length > maxLen)
            text = text[..maxLen] + "…";
        return text;
    }
}
