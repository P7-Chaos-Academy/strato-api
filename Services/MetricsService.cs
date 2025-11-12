using stratoapi.Data;
using stratoapi.Dtos;
using stratoapi.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace stratoapi.Services;

/// <summary>
/// Default implementation of <see cref="IMetricsService"/> that uses
/// <see cref="ApplicationDbContext"/> to read and write metric types.
/// </summary>
public class MetricsService : IMetricsService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetricsService"/> class.
    /// </summary>
    /// <param name="context">The application database context used to access persisted metric types.</param>
    /// <param name="mapper">The AutoMapper instance used to map between DTOs and domain models.</param>
    public MetricsService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    /// <inheritdoc/>
    public async Task<List<MetricType>> GetAllMetricTypes()
    {
        return await _context.MetricTypes
            .Where(e => !e.IsDeleted)
            .ToListAsync();
    }
    
    /// <inheritdoc/>
    public async Task AddMetricType(MetricsDto metricType)
    {
        MetricType metricTypeEntity = _mapper.Map<MetricType>(metricType);
        await _context.MetricTypes.AddAsync(metricTypeEntity);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task DeleteMetricType(int id)
    {
        MetricType metricType = await _context.MetricTypes.FindAsync(id);
        if (metricType == null)
        {
            throw new KeyNotFoundException($"Metric type with ID {id} not found.");
        }
        metricType.IsDeleted = true;
        await _context.SaveChangesAsync();
    }
}