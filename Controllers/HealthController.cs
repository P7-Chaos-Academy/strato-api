using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using StudiestartBackend.Models;

namespace StudiestartBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController
{
    [HttpGet]
    public async Task<String> Health()
    {
        return "Ok";
    }
}