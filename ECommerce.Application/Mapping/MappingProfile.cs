using AutoMapper;
using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.DTOs.Category;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.DTOs.OrderItem;
using ECommerce.Application.DTOs.Product;
using ECommerce.Application.DTOs.Wishlist;
using ECommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Entity -> DTO (For Reading)
            CreateMap<Product, ProductResponseDto>()
                .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category != null ? s.Category.Name : ""));

            // DTO -> Entity (For Creating)
            // Note: If names match, AutoMapper does it automatically.
            CreateMap<ProductCreateDto, Product>();

            CreateMap<ProductUpdateDto, Product>()
                .ForMember(d => d.Stock, o => o.Ignore());

            // Complex Mapping: Category -> DTO
            CreateMap<Category, CategoryResponseDto>();
            // Add these to your existing MappingProfile constructor
            CreateMap<CategoryCreateDto, Category>();

            CreateMap<CategoryUpdateDto, Category>();

            CreateMap<Order, OrderResponseDto>()
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.GetTotal()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : null))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : null))
                .ForMember(dest => dest.ShippingAddress, opt => opt.MapFrom(src => src.ShippingAddress))
                .ForMember(dest => dest.ShippingPhone, opt => opt.MapFrom(src => src.ShippingPhone));

            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product!.Name));

            CreateMap<CartItem, CartItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product!.Name))
                .ForMember(dest => dest.AvailableStock, opt => opt.MapFrom(src => src.Product != null ? src.Product.Stock : 0));

            CreateMap<WishlistItem, WishlistItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product != null ? src.Product.Price : 0m))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Product != null ? src.Product.ImageUrl : null))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Product != null ? src.Product.Description : string.Empty))
                .ForMember(dest => dest.AvailableStock, opt => opt.MapFrom(src => src.Product != null ? src.Product.Stock : 0));

            // Update DTO -> Entity (Status is set via SetStatus in service)
            CreateMap<OrderUpdateDto, Order>().ForMember(d => d.Status, o => o.Ignore());

        }
    }
}
