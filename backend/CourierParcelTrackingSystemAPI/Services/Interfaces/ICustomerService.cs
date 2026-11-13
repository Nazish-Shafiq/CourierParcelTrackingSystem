
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.CustomerDTO;

namespace CourierParcelTrackingSystem.API.Services.Interfaces;

public interface ICustomerService
{
    // Get customers with pagination and search.
    Task<PagedResponseDto<CustomerResponseDto>> GetPagedAsync(
        PaginationRequestDto pagination,
        string? search);

    // Get customer by ID.
    Task<CustomerResponseDto?> GetByIdAsync(int id);

    // Create customer.
    Task<CustomerResponseDto> CreateAsync(
    CreateCustomerRequestDto request,
    int? createdBy);

    //Update customer
    Task<CustomerResponseDto?> UpdateAsync(
        int id,
        UpdateCustomerRequestDto request,
        int? updatedBy);

    //Soft Delete customer
    Task<bool> SoftDeleteAsync(int id, int? updatedBy);

}
