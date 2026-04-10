using Kodvian.Core.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace Kodvian.Core.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult<ApiResponseDto<object>> Get()
    {
        return Ok(ApiResponseDto<object>.Ok(new { status = "disponible" }, "Servicio disponible"));
    }
}
