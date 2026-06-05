using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AvaloniaApplication1.Models;

namespace AvaloniaApplication1.Services;

public class GiteeReleaseService : IReleaseService
{
    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(15) };

    public async Task<List<ReleaseItem>> GetReleasesAsync(string owner, string repo, int page, int perPage)
    {
        var url = $"https://gitee.com/api/v5/repos/{owner}/{repo}/releases?page={page}&per_page={perPage}";
        var json = await _http.GetStringAsync(url);
        var giteeReleases = JsonSerializer.Deserialize<List<GiteeRelease>>(json, JsonOptions.Default)
                            ?? new List<GiteeRelease>();

        return giteeReleases.ConvertAll(r =>
        {
            var assets = new List<(string fileName, string downloadUrl, long size)>();
            if (r.Assets is not null)
            {
                foreach (var a in r.Assets)
                {
                    assets.Add((a.Name ?? "", a.BrowserDownloadUrl ?? "", a.Size));
                }
            }

            // Gitee Release 通常没有上传二进制附件，补充源码归档链接
            if (assets.Count == 0 && !string.IsNullOrEmpty(r.TagName))
            {
                var tag = r.TagName;
                assets.Add(($"frp_{tag}_source.zip", $"https://gitee.com/{owner}/{repo}/archive/refs/tags/{tag}.zip", 0));
                assets.Add(($"frp_{tag}_source.tar.gz", $"https://gitee.com/{owner}/{repo}/archive/refs/tags/{tag}.tar.gz", 0));
            }

            return new ReleaseItem(
                r.TagName ?? "",
                r.Name ?? r.TagName ?? "",
                r.CreatedAt,
                r.Body ?? "",
                assets
            );
        });
    }

    private class GiteeRelease
    {
        [JsonPropertyName("tag_name")] public string? TagName { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("body")] public string? Body { get; set; }
        [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; set; }
        [JsonPropertyName("assets")] public List<GiteeAsset>? Assets { get; set; }
    }

    private class GiteeAsset
    {
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("browser_download_url")] public string? BrowserDownloadUrl { get; set; }
        [JsonPropertyName("size")] public long Size { get; set; }
    }

    private static class JsonOptions
    {
        public static readonly JsonSerializerOptions Default = new() { PropertyNameCaseInsensitive = true };
    }
}
