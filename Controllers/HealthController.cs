using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using stratoapi.Models;

namespace stratoapi.Controllers;

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