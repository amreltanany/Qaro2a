using ECommerce.Application.DTOs.Order;

namespace ECommerce.API.ViewModels;

public class TrackViewModel
{
    public string? OrderNumber { get; set; }
    public string? Phone { get; set; }
    public bool NotFound { get; set; }
    public bool JustPlaced { get; set; }
    public OrderResponseDto? Order { get; set; }
}
