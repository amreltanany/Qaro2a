using AutoMapper;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.DTOs.OrderItem;
using ECommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Mapping
{
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile() {
            CreateMap<OrderItem, OrderItemDto>().ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product!.Name));
            // Create DTO -> Entity logic is usually handled manually in the 
            // service to ensure business rules are followed.
        }
    }
}
