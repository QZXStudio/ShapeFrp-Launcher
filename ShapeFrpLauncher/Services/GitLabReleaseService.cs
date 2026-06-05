using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AvaloniaApplication1.Models;

namespace AvaloniaApplication1.Services;

public class GitLabReleaseService : IReleaseService
{
    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(15) };

    public async Task<List<ReleaseItem>> GetReleasesAsync(string owner, string repo, int page, int perPage)
    {
        var projectPath = Uri.EscapeDataString($"{owner}/{repo}");
        var url = $"https://gitlab.com/api/v4/projects/{projectPath}/releases?page={page}&per_page={perPage}";
        var json = await _http.GetStringAsync(url);
        var gitlabReleases = JsonSerializer.Deserialize<List<GitLabRelease>>(json, JsonOptions.Default)
                             ?? new List<GitLabRelease>();

        return gitlabReleases.ConvertAll(r =>
        {
            var assets = new List<(string fileName, string downloadUrl, long size)>();
            if (r.Assets?.Links is not null)
            {
                foreach (var l in r.Assets.Links)
                {
                    assets.Add((l.Name ?? "", l.Url ?? "", 0));
                }
            }

            return new ReleaseItem(
                r.TagName ?? "",
                r.Name ?? r.TagName ?? "",
                r.ReleasedAt,
                r.Description ?? "",
                assets
            );
        });
    }

    private class GitLabRelease
    {
        [JsonPropertyName("tag_name")] public string? TagName { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }
        [JsonPropertyName("released_at")] public DateTimeOffset ReleasedAt { get; set; }
        [JsonPropertyName("assets")] public GitLabAssets? Assets { get; set; }
    }

    private class GitLabAssets
    {
        [JsonPropertyName("links")] public List<GitLabLink>? Links { get; set; }
    }

    private class GitLabLink
    {
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("url")] public string? Url { get; set; }
    }

    private static class JsonOptions
    {
        public static readonly JsonSerializerOptions Default = new() { PropertyNameCaseInsensitive = true };
    }
}
