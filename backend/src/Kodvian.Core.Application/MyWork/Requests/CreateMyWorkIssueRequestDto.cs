namespace Kodvian.Core.Application.MyWork.Requests;

public class CreateMyWorkIssueRequestDto
{
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Priority { get; set; }
}
