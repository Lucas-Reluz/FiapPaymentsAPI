using Microsoft.AspNetCore.Mvc;

namespace PaymentsAPI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("Health check called");
        return Ok(new { status = "Healthy", service = "PaymentsAPI", timestamp = DateTime.UtcNow });
    }
}
