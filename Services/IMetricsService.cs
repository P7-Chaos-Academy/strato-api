using stratoapi.Dtos;
using stratoapi.Models;

namespace stratoapi.Services;

/// <summary>
/// Defines operations for working with metric types.
/// Implementations should provide methods to query and modify persisted metric type data.
/// </summary>
public interface IMetricsService
{
    /// <summary>
    /// Retrieves all metric types from the data store.
    /// </summary>
    /// <returns>A list containing all metric types. The returned list may be empty but will never be <c>null</c>.</returns>
    Task<List<MetricType>> GetAllMetricTypes();
    
    /// <summary>
    /// Adds a new metric type to the data store.
    /// </summary>
    /// <param name="metricType">The metric type DTO to add. Must not be <c>null</c> and should contain all required properties.</param>
    /// <remarks>
    /// The CreatedBy field will be automatically set from the authenticated user's context.
    /// The CreatedAt field will be automatically set to the current UTC time.
    /// </remarks>
    Task AddMetricType(MetricsDto metricType);
    
    /// <summary>
    /// Soft deletes a metric type by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the metric type to delete.</param
    Task DeleteMetricType(int id);

    /// <summary>
    /// Updates an existing metric type.
    /// </summary>
    /// <param name="id">The unique identifier of the metric type to update.</param>
    /// <param name="metricType">The metric type DTO containing updated values.</param>
    /// <returns>The updated metric type, or <c>null</c> if no metric type with the given ID exists.</returns>
    Task<MetricType?> UpdateMetricType(int id, MetricsDto metricType);
}
