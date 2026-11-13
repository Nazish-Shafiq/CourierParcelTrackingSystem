
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.Entities;

namespace CourierParcelTrackingSystem.API.Repositories.Interfaces;

public interface ICustomerRepository
{
    // Get customers with pagination and search.
    Task<PagedResponseDto<Customer>> GetPagedAsync(
        PaginationRequestDto pagination,
        string? search);

    // Get customer by ID.
    Task<Customer?> GetByIdAsync(int id);

    // Check duplicate email.
    Task<bool> EmailExistsAsync(
        string email,
        int? excludeCustomerId = null);

    // Create customer.
    Task AddAsync(Customer customer);

    //Update customer
    Task UpdateAsync(Customer customer);

    //Soft Delete customer
    Task SoftDeleteAsync(Customer customer);

}
