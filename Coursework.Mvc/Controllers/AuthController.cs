using Coursework.Abstractions.Services;
using Coursework.Abstractions;
using Coursework.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

[Route("auth")]
public class AuthController : Controller
{
    private readonly JWTSettings _jwtSettings;
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, IOptions<JWTSettings> jwtSettings, ILogger<AuthController> logger)
    {
        _userService = userService;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("jwtToken");
        return RedirectToAction("Login", "Auth");
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        return View(new LoginRequest());
    }

    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequest model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var token = await _userService.AuthenticateAsync(model.Email, model.Password);
            _logger.LogInformation($"User '{model.Email}' logged in successfully.");

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddHours(1)
            };
            Response.Cookies.Append("jwtToken", token, cookieOptions);

            return RedirectToAction("Index", "Home");
        }
        catch (UnauthorizedAccessException)
        {
            ModelState.AddModelError(string.Empty, "Invalid credentials");
            return View(model);
        }
    }


    [HttpGet("register")]
    public IActionResult Register()
    {
        return View(new RegisterRequest());
    }

    [HttpPost("register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterRequest model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var existingUser = await _userService.GetByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email is already in use.");
                return View(model);
            }
            string role = "Customer";
            if (model.IsAdmin)
            {
                if (model.AdminSecretKey != "adminsecretkey")
                {
                    ModelState.AddModelError("AdminSecretKey", "Invalid Admin Secret Key.");
                    return View(model);
                }
                role = "admin";
            }
            UserRole userRole;
            if (!Enum.TryParse(role, ignoreCase: true, out userRole))
            {
                ModelState.AddModelError("Role", "Invalid role specified.");
                return View(model);
            }
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
            var newUser = new UserEntity
            {
                Username = model.Username,
                PasswordHash = hashedPassword,
                Email = model.Email,
                Role = userRole
            };

            await _userService.AddAsync(newUser);
            var token = await _userService.GenerateJwtToken(newUser);

            return RedirectToAction("Index", "Home");
        }
        catch (ArgumentNullException argEx)
        {
            _logger.LogError(argEx, "ArgumentNullException: Missing required argument.");
            ModelState.AddModelError(string.Empty, "Bad request: Missing required data.");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unhandled exception in Register method: {ex.Message}");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}

public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}
public class RegisterRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public bool IsAdmin { get; set; }
    public string? AdminSecretKey { get; set; }
}