using Kodvian.Core.Application.Common.Models;

namespace Kodvian.Core.Application.MyWork.Requests;

public class MyWorkIssueListRequestDto : PagedRequestDto
{
    public string? Search { get; set; }
    public Guid? ProjectId { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
}
