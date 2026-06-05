using System;
using System.IO;
using System.Text.Json;

namespace AvaloniaApplication1.Services;

public enum ReleaseSourceType
{
    GitHub,
    Gitee,
    GitLab
}

public static class ReleaseSourceConfig
{
    private static readonly string ConfigPath = Path.Combine(
        AppContext.BaseDirectory, "config.json");

    public static ReleaseSourceType CurrentSource { get; set; } = ReleaseSourceType.GitHub;
    public static string? GitHubToken { get; set; }
    public const string DefaultOwner = "fatedier";
    public const string DefaultRepo = "frp";

    public static void Load()
    {
        try
        {
            if (!File.Exists(ConfigPath)) return;
            var json = File.ReadAllText(ConfigPath);
            var data = JsonSerializer.Deserialize<ConfigData>(json);
            if (data is null) return;
            CurrentSource = data.Source;
            GitHubToken = data.GitHubToken;
        }
        catch { /* 配置损坏则忽略，使用默认值 */ }
    }

    public static void Save()
    {
        try
        {
            var data = new ConfigData
            {
                Source = CurrentSource,
                GitHubToken = GitHubToken
            };
            var json = JsonSerializer.Serialize(data);
            File.WriteAllText(ConfigPath, json);
        }
        catch { /* 写入失败静默忽略 */ }
    }

    private class ConfigData
    {
        public ReleaseSourceType Source { get; set; } = ReleaseSourceType.GitHub;
        public string? GitHubToken { get; set; }
    }
}
