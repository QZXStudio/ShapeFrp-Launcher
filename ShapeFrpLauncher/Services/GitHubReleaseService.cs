using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;

namespace AvaloniaApplication1.Services;

public class GitHubReleaseService
{
    private readonly GitHubClient _client;

    public GitHubReleaseService()
    {
        _client = new GitHubClient(new ProductHeaderValue("QZXFrp"));
    }

    public async Task<IReadOnlyList<Release>> GetReleasesAsync(string owner, string repo, int page = 1, int perPage = 20)
    {
        return await _client.Repository.Release.GetAll(owner, repo,
            new ApiOptions { PageSize = perPage, StartPage = page });
    }
}
