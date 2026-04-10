using Kodvian.Core.Application.Administration.Dtos;
using Kodvian.Core.Application.Common.Models;
using Kodvian.Core.Application.Common.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kodvian.Core.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Policy = "AdministrationRead", Roles = RoleNames.Administrator)]
public class UsersController : ControllerBase
{
    [HttpGet]
    public ActionResult<ApiResponseDto<PagedResultDto<UserListItemDto>>> Get([FromQuery] PagedRequestDto request)
    {
        var data = new PagedResultDto<UserListItemDto>
        {
            Items = Array.Empty<UserListItemDto>(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = 0
        };

        return Ok(ApiResponseDto<PagedResultDto<UserListItemDto>>.Ok(data, "Usuarios obtenidos correctamente"));
    }
}
