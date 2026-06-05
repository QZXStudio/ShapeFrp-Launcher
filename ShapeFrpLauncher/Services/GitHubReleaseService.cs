using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvaloniaApplication1.Models;
using Octokit;

namespace AvaloniaApplication1.Services;

public class GitHubReleaseService : IReleaseService
{
    private readonly GitHubClient _client;

    public GitHubReleaseService()
    {
        _client = string.IsNullOrEmpty(ReleaseSourceConfig.GitHubToken)
            ? new GitHubClient(new ProductHeaderValue("QZXFrp"))
            : new GitHubClient(new ProductHeaderValue("QZXFrp"))
            {
                Credentials = new Credentials(ReleaseSourceConfig.GitHubToken)
            };
    }

    /// <summary>验证 Token 是否有效，调用 /user 接口检查</summary>
    public static async Task<(bool ok, string message)> VerifyTokenAsync(string token)
    {
        try
        {
            var client = new GitHubClient(new ProductHeaderValue("QZXFrp"))
            {
                Credentials = new Credentials(token)
            };
            var user = await client.User.Current();
            return (true, $"Token 有效，用户：{user.Login}");
        }
        catch (AuthorizationException)
        {
            return (false, "Token 无效或已过期，请重新生成");
        }
        catch (RateLimitExceededException)
        {
            return (false, "GitHub API 速率限制已达，请稍后再试");
        }
        catch (Exception ex)
        {
            return (false, $"验证失败：{ex.Message}");
        }
    }

    public async Task<List<ReleaseItem>> GetReleasesAsync(string owner, string repo, int page, int perPage)
    {
        var releases = await _client.Repository.Release.GetAll(owner, repo,
            new ApiOptions { PageSize = perPage, StartPage = page });

        return releases.Select(r => new ReleaseItem(r, r.Assets)).ToList();
    }
}
