using Kodvian.Core.Application.Common.Models;

namespace Kodvian.Core.Application.MyWork.Requests;

public class MyWorkRepositoryListRequestDto : PagedRequestDto
{
    public string? Search { get; set; }
}
