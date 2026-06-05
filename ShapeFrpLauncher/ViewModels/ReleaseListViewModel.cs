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
    private const string Owner = "fatedier";
    private const string Repo = "frp";
    private const int PageSize = 20;

    private readonly GitHubReleaseService _service = new();

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

    private void RefreshComputed()
    {
        OnPropertyChanged(nameof(ShowEndLabel));
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

    /// <summary>静默刷新首页数据，不显示加载状态、不清空已有列表</summary>
    public async Task RefreshAsync()
    {
        try
        {
            var releases = await _service.GetReleasesAsync(Owner, Repo, 1, PageSize)
                .ConfigureAwait(false);

            var items = releases.Select(release =>
            {
                var winAsset = release.Assets.FirstOrDefault(a =>
                    a.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) &&
                    a.Name.Contains("windows", StringComparison.OrdinalIgnoreCase) &&
                    a.Name.Contains("amd64", StringComparison.OrdinalIgnoreCase));
                return new ReleaseItem(release, winAsset);
            }).ToList();

            var hasMore = releases.Count == PageSize;

            Dispatcher.UIThread.Post(() =>
            {
                Releases.Clear();
                foreach (var item in items)
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
            var releases = await _service.GetReleasesAsync(Owner, Repo, _currentPage + 1, PageSize)
                .ConfigureAwait(false);

            var items = releases.Select(release =>
            {
                var winAsset = release.Assets.FirstOrDefault(a =>
                    a.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) &&
                    a.Name.Contains("windows", StringComparison.OrdinalIgnoreCase) &&
                    a.Name.Contains("amd64", StringComparison.OrdinalIgnoreCase));
                return new ReleaseItem(release, winAsset);
            }).ToList();

            var page = _currentPage + 1;
            var hasMore = releases.Count == PageSize;

            Dispatcher.UIThread.Post(() =>
            {
                foreach (var item in items)
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
