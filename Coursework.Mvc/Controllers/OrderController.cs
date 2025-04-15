using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Coursework.Abstractions.Services;
using Coursework.Entities;

[Authorize]
public class OrderController : Controller
{
    private readonly IOrderService _orderService;
    private readonly IServiceService _serviceService;
    private readonly IOrderServiceService _orderServiceService;
    private readonly ILogger<OrderController> _logger;
    public OrderController(
        IOrderService orderService,
        IServiceService serviceService,
        IOrderServiceService orderServiceService,
        ILogger<OrderController> logger)
    {
        _orderService = orderService;
        _serviceService = serviceService;
        _orderServiceService = orderServiceService;
        _logger = logger;
    }
    public async Task<IActionResult> Index()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                Console.WriteLine("User ID claim not found.");
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);
            Console.WriteLine($"Получен userId: {userId}");

            var orders = await _orderService.GetOrdersByUserIdAsync(userId);

            Console.WriteLine($"Найдено заказов: {orders?.Count()}");

            return View(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка в OrderController.Index: {ex.Message}", ex);
            return StatusCode(500, "Внутренняя ошибка сервера.");
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null) return NotFound();

        var services = await _orderServiceService.GetServicesByOrderIdAsync(id);
        ViewBag.Services = services;
        return View(order);
    }

    public async Task<IActionResult> Create()
    {
        var services = await _serviceService.GetAllAsync();
        ViewBag.Services = services;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int[] selectedServiceIds)
    {
        if (!selectedServiceIds.Any())
        {
            ModelState.AddModelError("", "Выберите хотя бы один сервис.");
            ViewBag.Services = await _serviceService.GetAllAsync();
            return View();
        }

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var allServices = await _serviceService.GetAllAsync();
        var selectedServices = allServices.Where(s => selectedServiceIds.Contains(s.Id)).ToList();

        var order = new OrderEntity
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            Price = selectedServices.Sum(s => s.DiscountPrice > 0 ? s.DiscountPrice : s.DefaultPrice)
        };

        await _orderService.CreateOrderAsync(order);

        foreach (var service in selectedServices)
        {
            decimal price = service.DiscountPrice > 0 ? service.DiscountPrice : service.DefaultPrice;
            await _orderServiceService.AddServiceToOrderAsync(order.Id, service.Id, price);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null) return NotFound();

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var isAdmin = User.IsInRole("Admin");
        if (order.UserId != userId && !isAdmin) return Forbid();

        await _orderService.DeleteOrderAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
