using Kodvian.Core.Application.Integrations.GitHub;
using Kodvian.Core.Application.Integrations.GitHub.Abstractions;
using Kodvian.Core.Infrastructure.Integrations.GitHub;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Kodvian.Core.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/webhooks")]
public class GitHubWebhookController : ControllerBase
{
    private readonly IGitHubWebhookService _gitHubWebhookService;
    private readonly GitHubOptions _gitHubOptions;

    public GitHubWebhookController(IGitHubWebhookService gitHubWebhookService, IOptions<GitHubOptions> gitHubOptions)
    {
        _gitHubWebhookService = gitHubWebhookService;
        _gitHubOptions = gitHubOptions.Value;
    }

    [HttpPost("github")]
    public async Task<IActionResult> HandleGitHubWebhook(CancellationToken cancellationToken)
    {
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, leaveOpen: true);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        Request.Body.Position = 0;

        var signature = Request.Headers["X-Hub-Signature-256"].FirstOrDefault();
        if (!GitHubWebhookSignatureValidator.IsValid(payload, signature, _gitHubOptions.WebhookSecret))
        {
            return Unauthorized();
        }

        var eventName = Request.Headers["X-GitHub-Event"].FirstOrDefault();
        await _gitHubWebhookService.HandleIssueEventAsync(eventName, payload, cancellationToken);
        return Ok();
    }
}
