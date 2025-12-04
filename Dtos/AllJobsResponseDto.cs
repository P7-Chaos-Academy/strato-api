

using System.Text.Json.Serialization;

public class AllJobsResponseDto
{
    public List<JobStatus> Jobs { get; set; } = new List<JobStatus>();
}

public class JobStatus
{
    [JsonPropertyName("job_name")]
    public string JobName { get; set; } = string.Empty;
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    [JsonPropertyName("node_name")]
    public string? NodeName { get; set; }
    [JsonPropertyName("namespace")]
    public string? NameSpace { get; set; }
}
