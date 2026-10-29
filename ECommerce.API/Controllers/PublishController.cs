using ECommerce.API.Helpers;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.API.Controllers;

public class PublishController : Controller
{
    private const int MaxUploadsPerHour = 5;
    private const long RequestLimit = PdfUploadValidator.MaxBytes + 512 * 1024; // file + form fields

    private readonly IPublishRepository _publishRepository;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<PublishController> _logger;

    public PublishController(IPublishRepository publishRepository, IWebHostEnvironment environment, ILogger<PublishController> logger)
    {
        _publishRepository = publishRepository;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>Manuscripts live outside wwwroot so they can never be fetched by URL; only the admin download action serves them.</summary>
    public static string PrivateFolder(IWebHostEnvironment env) => Path.Combine(env.ContentRootPath, "PrivateUploads", "publish");

    private string? ResolveUserId() =>
        UserClaimsHelper.GetUserId(User)
        ?? UserClaimsHelper.GetUserIdFromJwtString(Request.Cookies["token"]);

    private static bool IsArabic => string.Equals(System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, "ar", StringComparison.OrdinalIgnoreCase);
    private static string T(string en, string ar) => IsArabic ? ar : en;

    // open to everyone: no account is needed to send a manuscript (like guest checkout)
    [HttpGet]
    public IActionResult Index() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("publish")]
    [RequestSizeLimit(RequestLimit)]
    [RequestFormLimits(MultipartBodyLengthLimit = RequestLimit)]
    public async Task<IActionResult> Index(string? fullName, string mobileNumber, IFormFile? pdfFile, CancellationToken cancellationToken = default)
    {
        var userId = ResolveUserId(); // null for a visitor without an account
        var errors = new List<string>();

        fullName = fullName?.Trim();
        if (string.IsNullOrWhiteSpace(fullName) || fullName.Length < 2)
            errors.Add(T("Please enter your name.", "من فضلك اكتب اسمك."));
        else if (fullName.Length > 100)
            errors.Add(T("The name is too long.", "الاسم طويل جدًا."));

        mobileNumber = (mobileNumber ?? string.Empty).Trim();
        if (mobileNumber.Length == 0)
            errors.Add(T("Mobile number is required.", "رقم الهاتف مطلوب."));
        else if (!PdfUploadValidator.IsValidPhone(mobileNumber))
            errors.Add(T("Enter a valid mobile number.", "أدخل رقم هاتف صحيح."));

        // at most a few submissions per user per hour, so the form cannot be used to flood the disk
        var since = DateTime.UtcNow.AddHours(-1);
        var recent = string.IsNullOrEmpty(userId) ? 0 : (await _publishRepository.GetByUserIdAsync(userId)).Count(p => p.CreatedAt > since);
        if (recent >= MaxUploadsPerHour)
            errors.Add(T("Too many submissions. Please try again later.", "عدد كبير من الطلبات. حاول مرة أخرى لاحقًا."));

        var folder = PrivateFolder(_environment);
        if (pdfFile == null || pdfFile.Length == 0)
            errors.Add(T("PDF file is required.", "ملف PDF مطلوب."));
        else if (errors.Count == 0)
        {
            var problem = await PdfUploadValidator.ValidateAsync(pdfFile, Path.Combine(folder, "quarantine"), _logger, cancellationToken);
            if (problem is { } p) errors.Add(T(p.En, p.Ar));
        }

        if (errors.Count > 0)
        {
            foreach (var err in errors)
                ModelState.AddModelError("", err);
            ViewBag.MobileNumber = mobileNumber;
            ViewBag.FullName = fullName;
            return View();
        }

        Directory.CreateDirectory(folder);
        var fileName = $"{Guid.NewGuid()}.pdf"; // server-chosen name: the client's file name is never used
        var filePath = Path.Combine(folder, fileName);
        await using (var stream = new FileStream(filePath, FileMode.CreateNew))
            await pdfFile!.CopyToAsync(stream, cancellationToken);

        var relativePath = $"/uploads/publish/{fileName}";
        var publish = new Publish(userId, mobileNumber, relativePath, fullName);
        await _publishRepository.AddAsync(publish);

        TempData["PublishSuccess"] = true;
        return RedirectToAction(nameof(Index));
    }
}
