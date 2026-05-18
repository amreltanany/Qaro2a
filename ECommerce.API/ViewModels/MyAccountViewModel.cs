using ECommerce.Application.DTOs.Order;

namespace ECommerce.API.ViewModels;

public class MyAccountViewModel
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<OrderResponseDto> Orders { get; set; } = new();
    public string? StatusMessage { get; set; }
    public string? ErrorMessage { get; set; }
}
