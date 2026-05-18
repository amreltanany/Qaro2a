using ECommerce.Application.DTOs.Contact;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

public class ContactController : Controller
{
    private readonly IContactRepository _contactRepository;

    public ContactController(IContactRepository contactRepository)
    {
        _contactRepository = contactRepository;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View("/Views/Components/Contact.cshtml", new ContactSubmissionDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(ContactSubmissionDto model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("/Views/Components/Contact.cshtml", model);
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
