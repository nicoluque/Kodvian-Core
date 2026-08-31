namespace Kodvian.Core.Application.Integrations.GitHub.Exceptions;

public class GitHubApiException : Exception
{
    public int StatusCode { get; }
    public string? ErrorCode { get; }

    public GitHubApiException(string message, int statusCode, string? errorCode = null)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}
