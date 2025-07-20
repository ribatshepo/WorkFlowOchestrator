using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.ComponentModel.DataAnnotations;
using WorkflowPlatform.API.Controllers;

namespace WorkflowPlatform.API.Controllers;

/// <summary>
/// Authentication and authorization controller
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Produces("application/json")]
[Tags("Authentication")]
public class AuthController : ApiControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate user and generate JWT token
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>JWT token and user information</returns>
    /// <response code="200">Authentication successful</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Invalid credentials</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Authentication attempt for username: {Username}", request.Username);

        // (In Production, you need to) Replace with actual user authentication logic
        // For now, using demo credentials for development
        if (!IsValidUser(request.Username, request.Password))
        {
            _logger.LogWarning("Failed authentication attempt for username: {Username}", request.Username);
            return Unauthorized(new ProblemDetails
            {
                Title = "Authentication Failed",
                Detail = "Invalid username or password",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        try
        {
            var token = await GenerateJwtTokenAsync(request.Username);
            
            _logger.LogInformation("Successfully authenticated user: {Username}", request.Username);

            return Ok(new LoginResponse
            {
                Token = token,
                TokenType = "Bearer",
                ExpiresIn = GetTokenExpirationMinutes() * 60, // Convert to seconds
                Username = request.Username,
                Roles = GetUserRoles(request.Username)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating token for user: {Username}", request.Username);
            return Problem("An error occurred during authentication");
        }
    }

    /// <summary>
    /// Refresh JWT token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>New JWT token</returns>
    /// <response code="200">Token refreshed successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Invalid or expired token</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Refresh([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var principal = GetPrincipalFromExpiredToken(request.Token);
            var username = principal?.Identity?.Name;

            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Token Refresh Failed",
                    Detail = "Invalid token",
                    Status = StatusCodes.Status401Unauthorized
                });
            }

            var newToken = await GenerateJwtTokenAsync(username);

            _logger.LogInformation("Token refreshed for user: {Username}", username);

            return Ok(new LoginResponse
            {
                Token = newToken,
                TokenType = "Bearer",
                ExpiresIn = GetTokenExpirationMinutes() * 60,
                Username = username,
                Roles = GetUserRoles(username)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return Unauthorized(new ProblemDetails
            {
                Title = "Token Refresh Failed",
                Detail = "Invalid or expired token",
                Status = StatusCodes.Status401Unauthorized
            });
        }
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    /// <returns>Current user details</returns>
    /// <response code="200">User information retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public ActionResult<UserInfoResponse> GetCurrentUser()
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized();
        }

        return Ok(new UserInfoResponse
        {
            Username = username,
            Roles = GetUserRoles(username),
            IsAuthenticated = true
        });
    }

    /// <summary>
    /// Logout user (for completeness - JWT tokens are stateless)
    /// </summary>
    /// <returns>Logout confirmation</returns>
    /// <response code="200">Logout successful</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult Logout()
    {
        var username = User.Identity?.Name;
        _logger.LogInformation("User logged out: {Username}", username);
        
        return Ok(new { message = "Logout successful" });
    }

    private async Task<string> GenerateJwtTokenAsync(string username)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
        var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
        var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(ClaimTypes.Name, username)
        };

        // Add role claims
        var roles = GetUserRoles(username);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
            signingCredentials: credentials);

        return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
        
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateLifetime = false // We don't validate lifetime here
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
        
        if (securityToken is not JwtSecurityToken jwtSecurityToken || 
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }

    private static bool IsValidUser(string username, string password)
    {
        //(In Production, you need to) Replace with actual user authentication logic
        // For development purposes, accept demo credentials
        return username switch
        {
            "admin" => password == "admin123",
            "user" => password == "user123",
            "demo" => password == "demo123",
            _ => false
        };
    }

    private static List<string> GetUserRoles(string username)
    {
        // (In Production, you need to) Replace with actual role lookup logic
        return username switch
        {
            "admin" => new List<string> { "Admin", "User" },
            "user" => new List<string> { "User" },
            "demo" => new List<string> { "User", "ReadOnly" },
            _ => new List<string> { "User" }
        };
    }

    private int GetTokenExpirationMinutes()
    {
        return _configuration.GetSection("Jwt").GetValue<int>("ExpirationInMinutes", 60);
    }
}

#region DTOs

/// <summary>
/// Login request model
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Username for authentication
    /// </summary>
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Password for authentication
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Login response model
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// JWT access token
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Token type (Bearer)
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Token expiration time in seconds
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Authenticated username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// User roles
    /// </summary>
    public List<string> Roles { get; set; } = new();
}

/// <summary>
/// Refresh token request model
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// Existing JWT token to refresh
    /// </summary>
    [Required]
    public string Token { get; set; } = string.Empty;
}

/// <summary>
/// User information response model
/// </summary>
public class UserInfoResponse
{
    /// <summary>
    /// Username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// User roles
    /// </summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>
    /// Authentication status
    /// </summary>
    public bool IsAuthenticated { get; set; }
}

#endregion
