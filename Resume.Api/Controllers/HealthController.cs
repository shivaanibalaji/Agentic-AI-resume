using Microsoft.AspNetCore.Mvc;
using Resume.Infrastructure.Persistence;

namespace Resume.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController(ResumeDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        bool databaseOnline;

        try
        {
            databaseOnline = await dbContext.Database.CanConnectAsync(cancellationToken);
        }
        catch
        {
            databaseOnline = false;
        }

        return Ok(new
        {
            status = "Healthy",
            utcTimestamp = DateTime.UtcNow,
            database = databaseOnline ? "online" : "offline"
        });
    }
}
