using Microsoft.AspNetCore.Mvc;
using Resume.Infrastructure.Persistence;

namespace Resume.Api.Controllers;

/// <summary>
/// Provides health check information about the API and its dependencies.
/// </summary>
[ApiController]
[Route("api/health")]
public class HealthController(ResumeDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// Returns the health status of the API and the database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The health status payload.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
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
