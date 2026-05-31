namespace ECommerce.Application.Services.Interfaces;

public interface IPasswordResetCodeService
{
    string GenerateAndStore(string email, string userId);
    bool TryValidate(string email, string code, out string? userId);
    void Remove(string email);
}
