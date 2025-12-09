namespace stratoapi.Models;

public class Cluster : BaseModel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string ApiEndpoint { get; set; }
    public string PrometheusEndpoint { get; set; }
}