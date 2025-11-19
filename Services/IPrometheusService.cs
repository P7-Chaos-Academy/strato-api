using stratoapi.Dtos;

namespace stratoapi.Services;

/// <summary>
/// Simplified wrapper around Prometheus HTTP API used by the application.
/// Implementations should translate the simplified <see cref="PrometheusQueryDto"/>
/// into the corresponding Prometheus HTTP request and return the raw JSON response.
/// </summary>
public interface IPrometheusService
{
    /// <summary>
    /// Executes a Prometheus instant or range query based on the provided DTO.
    /// Returns the raw Prometheus JSON response as a string.
    /// </summary>
    Task<string> QueryAsync(PrometheusQueryDto dto);
}
