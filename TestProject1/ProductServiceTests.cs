using AutoMapper;
using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services.Implementations;
using ECommerce.Domain.Entities;
using FluentValidation;
using Moq;
using Xunit;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly ProductService _service;
    private readonly Mock<IValidator<ProductUpdateDto>> _updateValidatorMock; // Add this
    private readonly Mock<IValidator<ProductCreateDto>> _createValidatorMock; // Add this

    public ProductServiceTests()
    {
        _productRepoMock = new Mock<IProductRepository>();
        _mapperMock = new Mock<IMapper>();
        _createValidatorMock = new Mock<IValidator<ProductCreateDto>>();
        _updateValidatorMock = new Mock<IValidator<ProductUpdateDto>>();

        // 2. Setup "Happy Path" by default so validators don't return null
        _createValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<ProductCreateDto>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _updateValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<ProductUpdateDto>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        // 3. Pass the .Object of all mocks
        _service = new ProductService(
            _productRepoMock.Object,
            _mapperMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
    {
        // Arrange: Setup the mock to return null
        _productRepoMock.Setup(repo => repo.GetByIdAsync(999))
                        .ReturnsAsync((Product)null);

        // Act: Call the service
        var result = await _service.GetByIdAsync(999);

        // Assert: Verify it returns null
        Assert.Null(result);
    }
    [Fact]
    public async Task AddAsync_ShouldThrowValidationException_WhenPriceIsNegative()
    {
        // Arrange
        var invalidDto = new ProductCreateDto { Name = "Test", Price = -10 };

        // Create a fake failed validation result
        var failures = new List<FluentValidation.Results.ValidationFailure>
    {
        new ("Price", "Price cannot be negative")
    };
        var validationResult = new FluentValidation.Results.ValidationResult(failures);

        // Setup the mock to return this failure
        _createValidatorMock.Setup(v => v.ValidateAsync(invalidDto, default))
                            .ReturnsAsync(validationResult);

        // Act & Assert
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => _service.AddAsync(invalidDto));
    }
    [Fact]
    public async Task UpdateAsync_ShouldThrowException_WhenIdsDoNotMatch()
    {
        // 1. Arrange
        int urlId = 1;
        var dtoWithDifferentId = new ProductUpdateDto { id = 99, Name = "Mismatched" };

        // 2. Act & Assert
        // This confirms your "if (id != dto.id)" check is alive
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            _service.UpdateAsync(urlId, dtoWithDifferentId));
    }
}   