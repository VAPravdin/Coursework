using Coursework.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Coursework.Abstractions.Services;
using System.IdentityModel.Tokens.Jwt;

public class ServiceController : Controller
{
    private readonly IServiceService _serviceService;

    public ServiceController(IServiceService serviceService)
    {
        _serviceService = serviceService;
    }

    public async Task<IActionResult> Index()
    {
        int currentUserId = GetUserIdFromToken(HttpContext);
        ViewBag.CurrentUserId = currentUserId;
        var services = await _serviceService.GetAllAsync();
        return View(services);
    }

    public async Task<IActionResult> Details(int id)
    {
        var service = await _serviceService.GetByIdAsync(id);
        if (service == null) return NotFound();
        return View(service);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceEntity service)
    {
        int userId;
        try
        {
            userId = GetUserIdFromToken(HttpContext);
        }
        catch
        {
            return Unauthorized();
        }

        service.UserId = userId;
        service.User = null;


        ModelState.Remove("User");
        ModelState.Remove("UserId");

        if (!ModelState.IsValid)
            return View(service);

        await _serviceService.AddAsync(service);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var service = await _serviceService.GetByIdAsync(id);
        if (service == null) return NotFound();
        return View(service);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _serviceService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
    public int GetUserIdFromToken(HttpContext httpContext)
    {
        var jwt = httpContext.Request.Cookies["jwtToken"];
        if (string.IsNullOrEmpty(jwt))
            throw new UnauthorizedAccessException("JWT token not found");

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);

        var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token");

        return userId;
    }
}
