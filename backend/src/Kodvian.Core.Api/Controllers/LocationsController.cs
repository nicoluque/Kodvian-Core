using Kodvian.Core.Application.Common.Models;
using Kodvian.Core.Application.Locations.Abstractions;
using Kodvian.Core.Application.Locations.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kodvian.Core.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/locations")]
public class LocationsController : ControllerBase
{
    private readonly ILocationService _locationService;

    public LocationsController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [HttpGet("countries")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<CountryOptionDto>>>> GetCountries(CancellationToken cancellationToken)
    {
        var data = await _locationService.GetCountriesAsync(cancellationToken);
        return Ok(ApiResponseDto<IReadOnlyCollection<CountryOptionDto>>.Ok(data, "Paises obtenidos correctamente"));
    }

    [HttpGet("regions")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<RegionOptionDto>>>> GetRegions([FromQuery] string countryCode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return BadRequest(ApiResponseDto<IReadOnlyCollection<RegionOptionDto>>.Fail("El pais es obligatorio"));
        }

        var data = await _locationService.GetRegionsAsync(countryCode.Trim(), cancellationToken);
        return Ok(ApiResponseDto<IReadOnlyCollection<RegionOptionDto>>.Ok(data, "Provincias obtenidas correctamente"));
    }

    [HttpGet("cities")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<CityOptionDto>>>> GetCities([FromQuery] string countryCode, [FromQuery] string regionCode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return BadRequest(ApiResponseDto<IReadOnlyCollection<CityOptionDto>>.Fail("El pais es obligatorio"));
        }

        if (string.IsNullOrWhiteSpace(regionCode))
        {
            return BadRequest(ApiResponseDto<IReadOnlyCollection<CityOptionDto>>.Fail("La provincia es obligatoria"));
        }

        var data = await _locationService.GetCitiesAsync(countryCode.Trim(), regionCode.Trim(), cancellationToken);
        return Ok(ApiResponseDto<IReadOnlyCollection<CityOptionDto>>.Ok(data, "Ciudades obtenidas correctamente"));
    }
}
