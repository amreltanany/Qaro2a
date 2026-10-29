using ECommerce.API.Filters;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[RequireAdmin]
public class AdminPublishesController : Controller
{
    private readonly IPublishRepository _publishRepository;
    private readonly IWebHostEnvironment _environment;

    public AdminPublishesController(IPublishRepository publishRepository, IWebHostEnvironment environment)
    {
        _publishRepository = publishRepository;
        _environment = environment;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var list = await _publishRepository.GetAllAsync();
        return View(list);
    }

    /// <summary>Admin-only download. Always sent as an attachment with nosniff so a manuscript is never rendered in the site's origin.</summary>
    public async Task<IActionResult> Download(int id)
    {
        var item = await _publishRepository.GetByIdAsync(id);
        if (item == null) return NotFound();

        var name = Path.GetFileName(item.PdfFilePath); // strips any directory part
        var path = Path.Combine(PublishController.PrivateFolder(_environment), name);
        if (!System.IO.File.Exists(path)) // manuscripts uploaded before the private folder existed
            path = Path.Combine(_environment.WebRootPath, "uploads", "publish", name);
        if (!System.IO.File.Exists(path)) return NotFound();

        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return PhysicalFile(path, "application/pdf", $"manuscript-{id}.pdf");
    }
}
