using Framework.Contracts.Common;
using Framework.Contracts.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Framework.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/health")]
public sealed class HealthController(HealthCheckService healthCheckService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(CommonResult<HealthCheckResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CommonResult<HealthCheckResponse>), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<CommonResult<HealthCheckResponse>>> Get(CancellationToken cancellationToken)
    {
        HealthReport report = await healthCheckService.CheckHealthAsync(cancellationToken);
        HealthCheckResponse response = new(
            report.Status.ToString(),
            DateTimeOffset.UtcNow,
            report.Entries.ToDictionary(entry => entry.Key, entry => entry.Value.Status.ToString()));

        CommonResult<HealthCheckResponse> result = CommonResult<HealthCheckResponse>.Success(response);

        return report.Status == HealthStatus.Healthy
            ? Ok(result)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, result);
    }
}

