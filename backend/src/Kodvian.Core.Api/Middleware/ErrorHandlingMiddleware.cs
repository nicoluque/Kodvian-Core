using System.Text.Json;
using Kodvian.Core.Application.Common.Models;
using Kodvian.Core.Application.Integrations.GitHub.Exceptions;

namespace Kodvian.Core.Api.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ArgumentException exception)
        {
            _logger.LogWarning(exception, "Validation error while processing request.");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var response = ApiResponseDto<object>.Fail(exception.Message);
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        catch (GitHubApiException exception)
        {
            _logger.LogWarning(exception, "GitHub API error while processing request.");
            context.Response.StatusCode = exception.StatusCode is >= 400 and < 500
                ? exception.StatusCode
                : StatusCodes.Status502BadGateway;
            context.Response.ContentType = "application/json";

            var response = ApiResponseDto<object>.Fail(exception.Message);
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled error while processing request.");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var response = ApiResponseDto<object>.Fail("Ocurrió un error interno al procesar la solicitud");
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
