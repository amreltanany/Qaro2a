using ECommerce.API.Helpers;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

public class PublishController : Controller
{
    private const long PdfMaxBytes = 5 * 1024 * 1024; // 5 MB
    private const string AllowedPdfContentType = "application/pdf";

    private readonly IPublishRepository _publishRepository;
    private readonly IWebHostEnvironment _environment;

    public PublishController(IPublishRepository publishRepository, IWebHostEnvironment environment)
    {
        _publishRepository = publishRepository;
        _environment = environment;
    }

    private string? ResolveUserId() =>
        UserClaimsHelper.GetUserId(User)
        ?? UserClaimsHelper.GetUserIdFromJwtString(Request.Cookies["token"]);

    [HttpGet]
    public IActionResult Index()
    {
        var userId = ResolveUserId();
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login", "Home", new { returnUrl = Url.Action(nameof(Index)) });

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(string mobileNumber, IFormFile? pdfFile, CancellationToken cancellationToken = default)
    {
        var userId = ResolveUserId();
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login", "Home", new { returnUrl = Url.Action(nameof(Index)) });

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(mobileNumber))
            errors.Add("Mobile number is required.");
        else
            mobileNumber = mobileNumber.Trim();

        if (pdfFile == null || pdfFile.Length == 0)
            errors.Add("PDF file is required.");
        else
        {
            if (pdfFile.Length > PdfMaxBytes)
                errors.Add("PDF file must be 5 MB or less.");
            var contentType = pdfFile.ContentType?.ToLowerInvariant() ?? "";
            if (contentType != AllowedPdfContentType && !contentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
            {
                var ext = Path.GetExtension(pdfFile.FileName)?.ToLowerInvariant();
                if (ext != ".pdf")
                    errors.Add("Only PDF files are allowed.");
            }
        }

        if (errors.Count > 0)
        {
            foreach (var err in errors)
                ModelState.AddModelError("", err);
            ViewBag.MobileNumber = mobileNumber;
            return View();
        }

        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "publish");
        Directory.CreateDirectory(uploadsFolder);
        var fileName = $"{Guid.NewGuid()}.pdf";
        var filePath = Path.Combine(uploadsFolder, fileName);
        await using (var stream = new FileStream(filePath, FileMode.Create))
            await pdfFile!.CopyToAsync(stream, cancellationToken);

        var relativePath = $"/uploads/publish/{fileName}";
        var publish = new Publish(userId, mobileNumber, relativePath);
        await _publishRepository.AddAsync(publish);

        TempData["PublishSuccess"] = true;
        return RedirectToAction(nameof(Index));
    }
}
