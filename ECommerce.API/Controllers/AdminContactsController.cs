using ECommerce.API.Filters;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[RequireAdmin]
public class AdminContactsController : Controller
{
    private readonly IContactRepository _contactRepository;

    public AdminContactsController(IContactRepository contactRepository)
    {
        _contactRepository = contactRepository;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var list = await _contactRepository.GetAllOrderedByNewestAsync(cancellationToken);
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _contactRepository.DeleteByIdAsync(id, cancellationToken);
        TempData["Success"] = deleted ? "Message deleted." : "Message was not found.";
        return RedirectToAction(nameof(Index));
    }
}
