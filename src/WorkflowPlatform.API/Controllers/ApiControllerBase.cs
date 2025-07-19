using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Diagnostics;

namespace WorkflowPlatform.API.Controllers;

/// <summary>
/// Base controller providing common functionality for all API controllers
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    private IMediator? _mediator;

    /// <summary>
    /// Gets the MediatR instance for handling commands and queries
    /// </summary>
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

    /// <summary>
    /// Creates a standardized error response
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="details">Additional error details</param>
    /// <returns>BadRequest result with error information</returns>
    protected IActionResult CreateErrorResponse(string message, object? details = null)
    {
        var errorResponse = new
        {
            Message = message,
            Details = details,
            Timestamp = DateTime.UtcNow,
            TraceId = HttpContext.TraceIdentifier
        };

        return BadRequest(errorResponse);
    }

    /// <summary>
    /// Creates a standardized success response
    /// </summary>
    /// <param name="data">Response data</param>
    /// <param name="message">Success message</param>
    /// <returns>Ok result with data</returns>
    protected IActionResult CreateSuccessResponse(object data, string? message = null)
    {
        var response = new
        {
            Success = true,
            Message = message,
            Data = data,
            Timestamp = DateTime.UtcNow
        };

        return Ok(response);
    }
}

/// <summary>
/// Health and system status controller
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class StatusController : ApiControllerBase
{
    /// <summary>
    /// Gets the API status and version information
    /// </summary>
    /// <returns>API status information</returns>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult GetStatus()
    {
        var status = new
        {
            Service = "Workflow Platform API",
            Version = "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
            Status = "Running",
            Timestamp = DateTime.UtcNow,
            Uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime
        };

        return CreateSuccessResponse(status);
    }
}
