using System;

namespace AvaloniaApplication1.Services;

public enum ReleaseSourceType
{
    GitHub,
    Gitee,
    GitLab
}

public static class ReleaseSourceConfig
{
    public static ReleaseSourceType CurrentSource { get; set; } = ReleaseSourceType.GitHub;
    public static string? GitHubToken { get; set; }
    public const string DefaultOwner = "fatedier";
    public const string DefaultRepo = "frp";

    public static void Load()
    {
        CurrentSource = SettingsDb.ReadSource();
        GitHubToken = SettingsDb.ReadToken();
    }

    public static void Save()
    {
        SettingsDb.WriteSource(CurrentSource);
        SettingsDb.WriteToken(GitHubToken);
    }
}
