namespace stratoapi.Dtos;

public class MetricsDto
{
    public float CpuUsage { get; set; }
    public float MemoryUsage { get; set; }
    public float DiskUsage { get; set; }
    public float GpuUsage { get; set; }
    public float Temperature { get; set; }
    // And all the other metrics that we don't remember right now
}