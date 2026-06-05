using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaApplication1.Models;
using AvaloniaApplication1.Services;

namespace AvaloniaApplication1.ViewModels;

public class ReleaseListViewModel : ViewModelBase
{
    private const string GitHubOwner = "fatedier";
    private const string GitHubRepo = "frp";
    private const string GiteeOwner = "firfe";
    private const string GiteeRepo = "frp_zh";
    private const int PageSize = 20;

    private static (string owner, string repo) GetOwnerRepo(ReleaseSourceType source) => source switch
    {
        ReleaseSourceType.Gitee => (GiteeOwner, GiteeRepo),
        _ => (GitHubOwner, GitHubRepo),
    };

    private IReleaseService _service = ReleaseServiceFactory.Create(ReleaseSourceConfig.CurrentSource);

    public ObservableCollection<ReleaseItem> Releases { get; } = new();

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private bool _hasMore = true;
    public bool HasMore
    {
        get => _hasMore;
        set => SetProperty(ref _hasMore, value);
    }

    private bool _isEmpty;
    public bool IsEmpty
    {
        get => _isEmpty;
        set => SetProperty(ref _isEmpty, value);
    }

    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    private bool _hasError;
    public bool HasError
    {
        get => _hasError;
        set => SetProperty(ref _hasError, value);
    }

    public bool ShowEndLabel => !HasMore && !IsEmpty && !IsLoading;

    private int _currentPage;
    private static ReleaseSourceType? _previousSource;
    private static string? _previousToken;

    private void RefreshComputed()
    {
        OnPropertyChanged(nameof(ShowEndLabel));
    }

    /// <summary>
    /// 检查仓库来源或 Token 是否变更，若变更则重建服务并全量刷新。
    /// </summary>
    public async Task EnsureFreshAsync()
    {
        if (_previousSource != ReleaseSourceConfig.CurrentSource || _previousToken != ReleaseSourceConfig.GitHubToken)
        {
            _previousSource = ReleaseSourceConfig.CurrentSource;
            _previousToken = ReleaseSourceConfig.GitHubToken;
            _service = ReleaseServiceFactory.Create(ReleaseSourceConfig.CurrentSource);
        }

        if (Releases.Count > 0)
            await RefreshAsync();
        else
            await LoadFirstPageAsync();
    }

    public async Task LoadFirstPageAsync()
    {
        if (_isLoading) return;

        _currentPage = 0;
        Releases.Clear();
        HasError = false;
        ErrorMessage = null;
        HasMore = true;
        RefreshComputed();

        await LoadNextPageAsync();
    }

    public async Task ForceReloadAsync()
    {
        _service = ReleaseServiceFactory.Create(ReleaseSourceConfig.CurrentSource);
        await LoadFirstPageAsync();
    }

    /// <summary>静默刷新首页数据，不显示加载状态、不清空已有列表</summary>
    public async Task RefreshAsync()
    {
        try
        {
            var (owner, repo) = GetOwnerRepo(ReleaseSourceConfig.CurrentSource);
            var releases = await _service.GetReleasesAsync(owner, repo, 1, PageSize)
                .ConfigureAwait(false);

            var hasMore = releases.Count == PageSize;

            Dispatcher.UIThread.Post(() =>
            {
                Releases.Clear();
                foreach (var item in releases)
                    Releases.Add(item);
                _currentPage = 1;
                HasMore = hasMore;
                IsEmpty = Releases.Count == 0;
                RefreshComputed();
            });
        }
        catch
        {
            // 静默刷新失败不提示，保留旧数据
        }
    }

    public async Task LoadNextPageAsync()
    {
        if (_isLoading || !_hasMore) return;

        IsLoading = true;
        HasError = false;
        ErrorMessage = null;

        try
        {
            var (owner, repo) = GetOwnerRepo(ReleaseSourceConfig.CurrentSource);
            var releases = await _service.GetReleasesAsync(owner, repo, _currentPage + 1, PageSize)
                .ConfigureAwait(false);

            var page = _currentPage + 1;
            var hasMore = releases.Count == PageSize;

            Dispatcher.UIThread.Post(() =>
            {
                foreach (var item in releases)
                    Releases.Add(item);
                _currentPage = page;
                HasMore = hasMore;
                IsEmpty = Releases.Count == 0;
                IsLoading = false;
                RefreshComputed();
            });
        }
        catch (Exception ex)
        {
            var message = $"加载失败：{ex.Message}";
            Dispatcher.UIThread.Post(() =>
            {
                HasError = true;
                ErrorMessage = message;
                IsLoading = false;
            });
        }
    }
}
