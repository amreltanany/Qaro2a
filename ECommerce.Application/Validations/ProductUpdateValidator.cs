using ECommerce.Application.DTOs.Product;
using FluentValidation;

namespace ECommerce.Application.Validations
{
    public class ProductUpdateValidator : AbstractValidator<ProductUpdateDto>
    {
        public ProductUpdateValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.Price)
                .NotEmpty().WithMessage("Product Price is required.")
                .GreaterThan(0).WithMessage("Price must be greater than zero.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("A category must be assigned.");

            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0).WithMessage("Stock must be 0 or greater.");

            RuleFor(x => x.ImageUrl)
                .MaximumLength(2048).WithMessage("Image URL cannot exceed 2048 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));

            RuleFor(x => x.PublishDate)
                .NotEmpty().WithMessage("Publish date is required.");

            RuleFor(x => x.Author)
                .NotEmpty().WithMessage("Author is required.")
                .MaximumLength(100).WithMessage("Author cannot exceed 100 characters.");
        }
    }
}