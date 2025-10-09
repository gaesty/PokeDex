using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using BourgPalette.Models;
using BourgPalette.Constants;
using BourgPalette.DTOs;
using BourgPalette.Services;
using BourgPalette.Data;

namespace BourgPalette.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AuthController> _logger;
    private readonly ITokenService _tokenService;
    private readonly ApplicationDbContext _context;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        ILogger<AuthController> logger,
        ITokenService tokenService,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _logger = logger;
        _tokenService = tokenService;
        _context = context;
    }

    [HttpPost("signup")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Signup([FromBody] SignupModel model)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }
        try
        {
            var existingUser = await _userManager.FindByNameAsync(model.Email) ?? await _userManager.FindByEmailAsync(model.Email);
            if (existingUser is not null)
            {
                return BadRequest("User already exists");
            }


            var user = new ApplicationUser
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Email,
                Name = model.Name,
                EmailConfirmed = true
            };

            var createUserResult = await _userManager.CreateAsync(user, model.Password);
            if (!createUserResult.Succeeded)
            {
                var errors = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create user. Errors: {Errors}", errors);
                return BadRequest($"Failed to create user. Errors: {errors}");
            }

            // Roles removed per request – no assignment

            return CreatedAtAction(nameof(Signup), null);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }
        try
        {
            // Accept either username or email in the same field for convenience
            var user = await _userManager.FindByNameAsync(model.Username) ?? await _userManager.FindByEmailAsync(model.Username);
            if (user == null)
            {
                return BadRequest("User with this username is not registered with us.");
            }

            var isValidPassword = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isValidPassword)
            {
                return Unauthorized("Invalid credentials");
            }

            var authClaims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Roles removed – no role claims added

            var accessToken = _tokenService.GenerateAccessToken(authClaims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var tokenInfo = _context.TokenInfos.FirstOrDefault(a => a.Username == user.UserName);
            if (tokenInfo is null)
            {
                _context.TokenInfos.Add(new TokenInfo
                {
                    Username = user.UserName ?? string.Empty,
                    RefreshToken = refreshToken,
                    ExpiredAt = DateTime.UtcNow.AddDays(7)
                });
            }
            else
            {
                tokenInfo.RefreshToken = refreshToken;
                tokenInfo.ExpiredAt = DateTime.UtcNow.AddDays(7);
            }

            await _context.SaveChangesAsync();

            return Ok(new TokenModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed");
            return Unauthorized();
        }
    }

    [HttpPost("token/refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] TokenModel tokenModel)
    {
        try
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(tokenModel.AccessToken);
            var username = principal.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest("Invalid token principal.");
            }

            var tokenInfo = _context.TokenInfos.SingleOrDefault(u => u.Username == username);
            if (tokenInfo == null
                || tokenInfo.RefreshToken != tokenModel.RefreshToken
                || tokenInfo.ExpiredAt <= DateTime.UtcNow)
            {
                return BadRequest("Invalid refresh token. Please login again.");
            }

            var newAccessToken = _tokenService.GenerateAccessToken(principal.Claims);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            tokenInfo.RefreshToken = newRefreshToken; // rotating the refresh token
            await _context.SaveChangesAsync();

            return Ok(new TokenModel
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Me()
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(username)) return Unauthorized();
        var user = await _userManager.FindByNameAsync(username) ?? await _userManager.FindByEmailAsync(username);
        if (user is null) return Unauthorized();
        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new
        {
            user.Id,
            user.UserName,
            user.Email,
            user.Name,
            Roles = roles
        });
    }
    
    // Removed admin-only test endpoint (roles disabled)

    [HttpPost("token/revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke([FromBody] TokenModel tokenModel)
    {
        try
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(tokenModel.AccessToken);
            var username = principal.Identity?.Name;

            var user = _context.TokenInfos.SingleOrDefault(u => u.Username == username);
            if (user == null)
            {
                return BadRequest();
            }

            user.RefreshToken = string.Empty;
            await _context.SaveChangesAsync();

            return Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}