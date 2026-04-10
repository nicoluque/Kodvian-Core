using Kodvian.Core.Application.Auth.Dtos;

namespace Kodvian.Core.Application.Auth.Abstractions;

public interface ITokenService
{
    GeneratedTokenDto GenerateToken(TokenGenerationDto tokenGeneration);
}
