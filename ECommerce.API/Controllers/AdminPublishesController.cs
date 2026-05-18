using ECommerce.API.Filters;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[RequireAdmin]
public class AdminPublishesController : Controller
{
    private readonly IPublishRepository _publishRepository;

    public AdminPublishesController(IPublishRepository publishRepository)
    {
        _publishRepository = publishRepository;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var list = await _publishRepository.GetAllAsync();
        return View(list);
    }
}
