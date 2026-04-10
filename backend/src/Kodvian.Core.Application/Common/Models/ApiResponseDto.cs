namespace Kodvian.Core.Application.Common.Models;

public class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResponseDto<T> Ok(T data, string message) =>
        new()
        {
            Success = true,
            Message = message,
            Data = data
        };

    public static ApiResponseDto<T> Fail(string message) =>
        new()
        {
            Success = false,
            Message = message,
            Data = default
        };
}
