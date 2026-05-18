using ECommerce.Application.DTOs.Order;
using ECommerce.Application.DTOs.OrderItem;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Validations
{

    // Parent Validator
    public class CreateOrderValidator : AbstractValidator<OrderCreateDto>
    {
        public CreateOrderValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("An order must have at least one item.")
                .Must(items => items.Count > 0).WithMessage("Item list cannot be empty.");

            // This is the "Magic Line" that connects the two validators
            RuleForEach(x => x.Items).SetValidator(new CreateOrderItemValidator());
        }
    }

    // Child Validator (can stay in the same file)
    public class CreateOrderItemValidator : AbstractValidator<CreateOrderItemDto>
    {
        public CreateOrderItemValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Valid Product ID is required.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be at least 1.");
        }
    }
}
