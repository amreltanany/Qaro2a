using ECommerce.API.Helpers;
using ECommerce.Application.DTOs.Contact;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.API.Controllers;

public class ContactController : Controller
{
    private readonly IContactRepository _contactRepository;
    private readonly ContactSpamGuard _guard;
    private readonly ILogger<ContactController> _logger;

    public ContactController(IContactRepository contactRepository, ContactSpamGuard guard, ILogger<ContactController> logger)
    {
        _contactRepository = contactRepository;
        _guard = guard;
        _logger = logger;
    }

    private IActionResult Form(ContactSubmissionDto model)
    {
        ViewBag.FormToken = _guard.NewToken();
        return View("/Views/Components/Contact.cshtml", model);
    }

    [HttpGet]
    public IActionResult Index() => Form(new ContactSubmissionDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("contact")]
    public async Task<IActionResult> Send(ContactSubmissionDto model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Form(model);

        var verdict = _guard.Check(
            Request.Form[ContactSpamGuard.TrapField], Request.Form[ContactSpamGuard.TokenField],
            model.Name, model.Email, model.Subject, model.Message);

        if (verdict == ContactSpamGuard.Verdict.ReloadPage)
        {
            ModelState.AddModelError(string.Empty, "Please try sending again (the page was open for too long).");
            return Form(model);
        }
        if (verdict == ContactSpamGuard.Verdict.Spam)
        {
            // the robot sees the normal "sent" page; nothing is saved
            _logger.LogWarning("Contact spam dropped from {Ip}: {Email}", HttpContext.Connection.RemoteIpAddress, model.Email);
            TempData["ContactSent"] = true;
            return RedirectToAction(nameof(Index));
        }

        var entry = new Contact(
            model.Name.Trim(),
            model.Email.Trim(),
            model.Subject.Trim(),
            model.Message.Trim());

        await _contactRepository.AddAsync(entry, cancellationToken);
        TempData["ContactSent"] = true;
        return RedirectToAction(nameof(Index));
    }
}
