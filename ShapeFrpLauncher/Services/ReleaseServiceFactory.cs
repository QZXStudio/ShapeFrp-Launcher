namespace AvaloniaApplication1.Services;

public static class ReleaseServiceFactory
{
    public static IReleaseService Create(ReleaseSourceType source)
    {
        return source switch
        {
            ReleaseSourceType.GitHub => new GitHubReleaseService(),
            ReleaseSourceType.Gitee => new GiteeReleaseService(),
            ReleaseSourceType.GitLab => new GitLabReleaseService(),
            _ => new GitHubReleaseService()
        };
    }
}
