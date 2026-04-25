using Kodvian.Core.Application.Locations.Abstractions;
using Kodvian.Core.Application.Locations.Dtos;

namespace Kodvian.Core.Infrastructure.Services;

public class LocationService : ILocationService
{
    private static readonly IReadOnlyCollection<CountryOptionDto> Countries =
    [
        new CountryOptionDto { Code = "AR", Name = "Argentina" }
    ];

    private static readonly IReadOnlyCollection<RegionRecord> Regions =
    [
        new RegionRecord("AR", "AR-B", "Buenos Aires"),
        new RegionRecord("AR", "AR-K", "Catamarca"),
        new RegionRecord("AR", "AR-H", "Chaco"),
        new RegionRecord("AR", "AR-U", "Chubut"),
        new RegionRecord("AR", "AR-C", "Ciudad Autonoma de Buenos Aires"),
        new RegionRecord("AR", "AR-X", "Cordoba"),
        new RegionRecord("AR", "AR-W", "Corrientes"),
        new RegionRecord("AR", "AR-E", "Entre Rios"),
        new RegionRecord("AR", "AR-P", "Formosa"),
        new RegionRecord("AR", "AR-Y", "Jujuy"),
        new RegionRecord("AR", "AR-L", "La Pampa"),
        new RegionRecord("AR", "AR-F", "La Rioja"),
        new RegionRecord("AR", "AR-M", "Mendoza"),
        new RegionRecord("AR", "AR-N", "Misiones"),
        new RegionRecord("AR", "AR-Q", "Neuquen"),
        new RegionRecord("AR", "AR-R", "Rio Negro"),
        new RegionRecord("AR", "AR-A", "Salta"),
        new RegionRecord("AR", "AR-J", "San Juan"),
        new RegionRecord("AR", "AR-D", "San Luis"),
        new RegionRecord("AR", "AR-Z", "Santa Cruz"),
        new RegionRecord("AR", "AR-S", "Santa Fe"),
        new RegionRecord("AR", "AR-G", "Santiago del Estero"),
        new RegionRecord("AR", "AR-V", "Tierra del Fuego"),
        new RegionRecord("AR", "AR-T", "Tucuman")
    ];

    private static readonly IReadOnlyCollection<CityRecord> Cities =
    [
        new CityRecord("AR", "AR-B", "AR-B-LP", "La Plata"),
        new CityRecord("AR", "AR-B", "AR-B-MDP", "Mar del Plata"),
        new CityRecord("AR", "AR-B", "AR-B-BAH", "Bahia Blanca"),
        new CityRecord("AR", "AR-K", "AR-K-SFV", "San Fernando del Valle de Catamarca"),
        new CityRecord("AR", "AR-H", "AR-H-RES", "Resistencia"),
        new CityRecord("AR", "AR-U", "AR-U-RAW", "Rawson"),
        new CityRecord("AR", "AR-U", "AR-U-CRW", "Comodoro Rivadavia"),
        new CityRecord("AR", "AR-C", "AR-C-CABA", "Ciudad de Buenos Aires"),
        new CityRecord("AR", "AR-X", "AR-X-CBA", "Cordoba"),
        new CityRecord("AR", "AR-X", "AR-X-RCU", "Rio Cuarto"),
        new CityRecord("AR", "AR-W", "AR-W-CTE", "Corrientes"),
        new CityRecord("AR", "AR-E", "AR-E-PER", "Parana"),
        new CityRecord("AR", "AR-P", "AR-P-FMA", "Formosa"),
        new CityRecord("AR", "AR-Y", "AR-Y-SSJ", "San Salvador de Jujuy"),
        new CityRecord("AR", "AR-L", "AR-L-STA", "Santa Rosa"),
        new CityRecord("AR", "AR-F", "AR-F-LRJ", "La Rioja"),
        new CityRecord("AR", "AR-M", "AR-M-MDZ", "Mendoza"),
        new CityRecord("AR", "AR-M", "AR-M-SRA", "San Rafael"),
        new CityRecord("AR", "AR-N", "AR-N-POS", "Posadas"),
        new CityRecord("AR", "AR-Q", "AR-Q-NQN", "Neuquen"),
        new CityRecord("AR", "AR-R", "AR-R-VDM", "Viedma"),
        new CityRecord("AR", "AR-R", "AR-R-BRC", "San Carlos de Bariloche"),
        new CityRecord("AR", "AR-A", "AR-A-SLA", "Salta"),
        new CityRecord("AR", "AR-J", "AR-J-SJN", "San Juan"),
        new CityRecord("AR", "AR-D", "AR-D-SLU", "San Luis"),
        new CityRecord("AR", "AR-Z", "AR-Z-RGL", "Rio Gallegos"),
        new CityRecord("AR", "AR-S", "AR-S-SFE", "Santa Fe"),
        new CityRecord("AR", "AR-S", "AR-S-ROS", "Rosario"),
        new CityRecord("AR", "AR-G", "AR-G-SDE", "Santiago del Estero"),
        new CityRecord("AR", "AR-V", "AR-V-USH", "Ushuaia"),
        new CityRecord("AR", "AR-T", "AR-T-SMT", "San Miguel de Tucuman")
    ];

    public Task<IReadOnlyCollection<CountryOptionDto>> GetCountriesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Countries);
    }

    public Task<IReadOnlyCollection<RegionOptionDto>> GetRegionsAsync(string countryCode, CancellationToken cancellationToken = default)
    {
        var regions = Regions
            .Where(x => x.CountryCode.Equals(countryCode, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.Name)
            .Select(x => new RegionOptionDto
            {
                Code = x.Code,
                Name = x.Name
            })
            .ToList();

        return Task.FromResult<IReadOnlyCollection<RegionOptionDto>>(regions);
    }

    public Task<IReadOnlyCollection<CityOptionDto>> GetCitiesAsync(string countryCode, string regionCode, CancellationToken cancellationToken = default)
    {
        var cities = Cities
            .Where(x => x.CountryCode.Equals(countryCode, StringComparison.OrdinalIgnoreCase)
                     && x.RegionCode.Equals(regionCode, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.Name)
            .Select(x => new CityOptionDto
            {
                Code = x.Code,
                Name = x.Name
            })
            .ToList();

        return Task.FromResult<IReadOnlyCollection<CityOptionDto>>(cities);
    }

    private sealed record RegionRecord(string CountryCode, string Code, string Name);
    private sealed record CityRecord(string CountryCode, string RegionCode, string Code, string Name);
}
