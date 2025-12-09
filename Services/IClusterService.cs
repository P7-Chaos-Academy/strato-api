using stratoapi.Dtos;
using stratoapi.Models;

namespace stratoapi.Services;

public interface IClusterService 
{
    Task<List<Cluster>> GetAllClusters();
    
    Task AddCluster(ClusterDto cluster);
    
    Task DeleteCluster(int id);

    Task<Cluster?> UpdateCluster(int id, ClusterDto metricType);
    
    Task<string> GetClusterPrometheusEndpoint(int clusterId);
    
    Task<string> GetClusterApiEndpoint(int clusterId);
}