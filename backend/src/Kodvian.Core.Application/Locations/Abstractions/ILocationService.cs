using Kodvian.Core.Application.Locations.Dtos;

namespace Kodvian.Core.Application.Locations.Abstractions;

public interface ILocationService
{
    Task<IReadOnlyCollection<CountryOptionDto>> GetCountriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<RegionOptionDto>> GetRegionsAsync(string countryCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CityOptionDto>> GetCitiesAsync(string countryCode, string regionCode, CancellationToken cancellationToken = default);
}
