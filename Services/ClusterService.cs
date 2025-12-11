using Microsoft.EntityFrameworkCore;
using stratoapi.Data;
using stratoapi.Dtos;
using stratoapi.Models;

namespace stratoapi.Services;

public class ClusterService : IClusterService
{
    private readonly ILogger<ClusterService> _logger;
    private readonly ApplicationDbContext _dbContext;

    public ClusterService(ILogger<ClusterService> logger, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<List<Cluster>> GetAllClusters()
    {
        _logger.LogInformation("GetAllClusters called");
        var clusters = await _dbContext.Clusters
            .Where(e => !e.IsDeleted)
            .ToListAsync();
        _logger.LogInformation("Retrieved {ClusterCount} clusters", clusters.Count);
        return clusters;
    }

    public Task AddCluster(ClusterDto cluster)
    {
        _logger.LogInformation("AddCluster called with Name: {Name}, ApiEndpoint: {ApiEndpoint}",
            cluster.Name, cluster.ApiEndpoint);

        Cluster clusterEntity = new Cluster
        {
            Name = cluster.Name,
            Description = cluster.Description,
            ApiEndpoint = cluster.ApiEndpoint,
            PrometheusEndpoint = cluster.PrometheusEndpoint,
            IsDeleted = false
        };
        _dbContext.Clusters.Add(clusterEntity);
        return _dbContext.SaveChangesAsync();
    }

    public Task DeleteCluster(int id)
    {
        _logger.LogInformation("DeleteCluster called for cluster {ClusterId}", id);

        Cluster? clusterEntity = _dbContext.Clusters.Find(id);
        if (clusterEntity == null)
        {
            _logger.LogWarning("Cluster {ClusterId} not found for deletion", id);
            throw new KeyNotFoundException($"Cluster with ID {id} not found.");
        }
        clusterEntity.IsDeleted = true;
        _logger.LogInformation("Cluster {ClusterId} marked as deleted", id);
        return _dbContext.SaveChangesAsync();
    }

    public async Task<Cluster?> UpdateCluster(int id, ClusterDto cluster)
    {
        _logger.LogInformation("UpdateCluster called for cluster {ClusterId}", id);

        Cluster? existingCluster = _dbContext.Clusters.Find(id);
        if (existingCluster == null)
        {
            _logger.LogWarning("Cluster {ClusterId} not found for update", id);
            throw new KeyNotFoundException($"Cluster with ID {id} not found.");
        }

        existingCluster.Name = cluster.Name;
        existingCluster.Description = cluster.Description;
        existingCluster.ApiEndpoint = cluster.ApiEndpoint;
        existingCluster.PrometheusEndpoint = cluster.PrometheusEndpoint;

        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Cluster {ClusterId} updated successfully", id);
        return existingCluster;
    }

    public Task<string> GetClusterPrometheusEndpoint(int clusterId)
    {
        _logger.LogDebug("GetClusterPrometheusEndpoint called for cluster {ClusterId}", clusterId);

        Cluster? clusterEntity = _dbContext.Clusters.Find(clusterId);
        if (clusterEntity == null)
        {
            _logger.LogWarning("Cluster {ClusterId} not found when getting Prometheus endpoint", clusterId);
            throw new KeyNotFoundException($"Cluster with ID {clusterId} not found.");
        }

        return Task.FromResult(clusterEntity.PrometheusEndpoint);
    }

    public Task<string> GetClusterApiEndpoint(int clusterId)
    {
        _logger.LogDebug("GetClusterApiEndpoint called for cluster {ClusterId}", clusterId);

        Cluster? cluster = _dbContext.Clusters.Find(clusterId);
        if (cluster == null)
        {
            _logger.LogWarning("Cluster {ClusterId} not found when getting API endpoint", clusterId);
            throw new KeyNotFoundException($"Cluster with ID {clusterId} not found.");
        }

        return Task.FromResult(cluster.ApiEndpoint);
    }

    public Task<Dictionary<string, string>> CheckAllClustersHealth()
    {
        _logger.LogInformation("CheckAllClustersHealth called");

        var healthStatuses = new Dictionary<string, string>();
        var clusters = _dbContext.Clusters
            .Where(e => !e.IsDeleted)
            .ToList();

        foreach (var cluster in clusters)
        {
            // Placeholder logic for health check
            healthStatuses[cluster.Name] = "Healthy"; // In real implementation, perform actual health check
            _logger.LogInformation("Cluster {ClusterName} health status: {HealthStatus}", cluster.Name, healthStatuses[cluster.Name]);
        }

        return Task.FromResult(healthStatuses);
    }
}
