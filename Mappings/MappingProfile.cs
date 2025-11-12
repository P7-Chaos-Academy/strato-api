using AutoMapper;
using stratoapi.Dtos;
using stratoapi.Models;

namespace stratoapi.Mappings;

/// <summary>
/// AutoMapper profile for mapping between DTOs and domain models.
/// </summary>
public class MappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MappingProfile"/> class.
    /// </summary>
    public MappingProfile()
    {
        // MetricsDto -> MetricType
        CreateMap<MetricsDto, MetricType>();
    }
}

