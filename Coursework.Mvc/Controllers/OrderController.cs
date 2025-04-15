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

    public OrderController(
        IOrderService orderService,
        IServiceService serviceService,
        IOrderServiceService orderServiceService)
    {
        _orderService = orderService;
        _serviceService = serviceService;
        _orderServiceService = orderServiceService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var orders = await _orderService.GetOrdersByUserIdAsync(userId);
        return View(orders);
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
    public async Task<IActionResult> Create(List<int> selectedServiceIds)
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
