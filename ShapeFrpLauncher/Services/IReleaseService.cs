using System.Collections.Generic;
using System.Threading.Tasks;
using AvaloniaApplication1.Models;

namespace AvaloniaApplication1.Services;

public interface IReleaseService
{
    Task<List<ReleaseItem>> GetReleasesAsync(string owner, string repo, int page, int perPage);
}
