
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.CustomerDTO;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using CourierParcelTrackingSystem.API.Services.Interfaces;
using CourierParcelTrackingSystem.API.Entities;

namespace CourierParcelTrackingSystem.API.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;

    public CustomerService(ICustomerRepository repository)
    {
        _repository = repository;
    }

    // Get customers with pagination and optional search.
    public async Task<PagedResponseDto<CustomerResponseDto>> GetPagedAsync(
        PaginationRequestDto pagination,
        string? search)
    {
        var result = await _repository.GetPagedAsync(pagination, search);

        return new PagedResponseDto<CustomerResponseDto>
        {
            Items = result.Items.Select(Map).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalRecords = result.TotalRecords,
            TotalPages = result.TotalPages
        };
    }

    // Get customer by ID.
    public async Task<CustomerResponseDto?> GetByIdAsync(int id)
    {
        var customer = await _repository.GetByIdAsync(id);

        return customer is null ? null : Map(customer);
    }

    // Create a new customer.
    public async Task<CustomerResponseDto> CreateAsync(
        CreateCustomerRequestDto request,
        int? createdBy)
    {
        // Normalize email.
        var email = string.IsNullOrWhiteSpace(request.Email)
            ? null
            : request.Email.Trim().ToLowerInvariant();

        // Check duplicate email.
        if (email != null &&
            await _repository.EmailExistsAsync(email))
        {
            throw new InvalidOperationException(
                "A customer with this email already exists.");
        }

        // Generate unique customer code.
        var customerCode =
            $"CUS-{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}";

        // Create customer entity.
        var customer = new Customer
        {
            CustomerCode = customerCode,
            FullName = request.FullName.Trim(),
            Email = email,
            Phone = request.Phone.Trim(),
            AddressLine = Normalize(request.AddressLine),
            City = Normalize(request.City),
            Province = Normalize(request.Province),
            PostalCode = Normalize(request.PostalCode),

            // Default values.
            UserId = null,
            IsActive = true,
            IsDeleted = false,

            // Audit information.
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        // Save customer in database.
        await _repository.AddAsync(customer);

        return Map(customer);
    }

    // Update an existing customer.
    public async Task<CustomerResponseDto?> UpdateAsync(
        int id,
        UpdateCustomerRequestDto request,
        int? updatedBy)
    {
        // Find customer by ID.
        var customer = await _repository.GetByIdAsync(id);

        if (customer == null)
            return null;

        // Normalize email.
        var email = string.IsNullOrWhiteSpace(request.Email)
            ? null
            : request.Email.Trim().ToLowerInvariant();

        // Check duplicate email, excluding the current customer.
        if (email != null &&
            await _repository.EmailExistsAsync(email, id))
        {
            throw new InvalidOperationException(
                "A customer with this email already exists.");
        }

        // Update customer details.
        customer.FullName = request.FullName.Trim();
        customer.Email = email;
        customer.Phone = request.Phone.Trim();
        customer.AddressLine = Normalize(request.AddressLine);
        customer.City = Normalize(request.City);
        customer.Province = Normalize(request.Province);
        customer.PostalCode = Normalize(request.PostalCode);

        // Update audit information.
        customer.UpdatedAt = DateTime.UtcNow;
        customer.UpdatedBy = updatedBy;

        // Save changes.
        await _repository.UpdateAsync(customer);

        return Map(customer);
    }

    // Soft delete an existing customer.
    public async Task<bool> SoftDeleteAsync(int id, int? updatedBy)
    {
        // Find customer by ID.
        var customer = await _repository.GetByIdAsync(id);

        if (customer == null)
            return false;

        // Mark customer as deleted instead of removing the record.
        customer.IsActive = false;
        customer.IsDeleted = true;

        // Update audit information.
        customer.UpdatedAt = DateTime.UtcNow;
        customer.UpdatedBy = updatedBy;

        // Save changes.
        await _repository.SoftDeleteAsync(customer);

        return true;
    }

    // Convert empty optional strings to null.
    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    // Map Customer entity to CustomerResponseDto.
    private static CustomerResponseDto Map(Customer c) => new()
    {
        CustomerId = c.CustomerId,
        UserId = c.UserId,
        CustomerCode = c.CustomerCode,
        FullName = c.FullName,
        Email = c.Email,
        Phone = c.Phone,
        AddressLine = c.AddressLine,
        City = c.City,
        Province = c.Province,
        PostalCode = c.PostalCode,
        IsActive = c.IsActive,
        CreatedAt = c.CreatedAt
    };
}
