
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.CouriersDTO;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using CourierParcelTrackingSystem.API.Services.Interfaces;

namespace CourierParcelTrackingSystem.API.Services;

public class CourierService : ICourierService
{
    private readonly ICourierRepository _repository;

    public CourierService(ICourierRepository repository)
    {
        _repository = repository;
    }

    // Get couriers with pagination and optional search.
    public async Task<PagedResponseDto<CourierResponseDto>> GetPagedAsync(
        PaginationRequestDto pagination,
        string? search)
    {
        var result = await _repository.GetPagedAsync(pagination, search);

        return new PagedResponseDto<CourierResponseDto>
        {
            Items = result.Items.Select(Map).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalRecords = result.TotalRecords,
            TotalPages = result.TotalPages
        };
    }

    // Get courier by ID.
    public async Task<CourierResponseDto?> GetByIdAsync(int id)
    {
        var courier = await _repository.GetByIdAsync(id);

        return courier is null ? null : Map(courier);
    }

    // Create a new courier.
    public async Task<CourierResponseDto> CreateAsync(
        CreateCourierRequestDto request,
        int? createdBy)
    {
        // Generate a unique employee code.
        string employeeCode;

        do
        {
            employeeCode =
                $"COU-{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}";
        }
        while (await _repository.EmployeeCodeExistsAsync(employeeCode));

        // Create courier entity.
        var courier = new Courier
        {
            UserId = request.UserId,
            EmployeeCode = employeeCode,
            FullName = request.FullName.Trim(),
            Phone = request.Phone.Trim(),
            Email = Normalize(request.Email),
            VehicleType = Normalize(request.VehicleType),
            VehicleNumber = Normalize(request.VehicleNumber),

            // Default values.
            IsAvailable = true,
            IsActive = true,
            IsDeleted = false,

            // Audit information.
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        // Save courier in database.
        await _repository.AddAsync(courier);

        return Map(courier);
    }

    // Update an existing courier.
    public async Task<CourierResponseDto?> UpdateAsync(
        int id,
        UpdateCourierRequestDto request,
        int? updatedBy)
    {
        // Find courier by ID.
        var courier = await _repository.GetByIdAsync(id);

        if (courier == null)
            return null;

        // Update courier details.
        courier.UserId = request.UserId;
        courier.FullName = request.FullName.Trim();
        courier.Phone = request.Phone.Trim();
        courier.Email = Normalize(request.Email);
        courier.VehicleType = Normalize(request.VehicleType);
        courier.VehicleNumber = Normalize(request.VehicleNumber);
        courier.IsAvailable = request.IsAvailable;
        courier.IsActive = request.IsActive;

        // Update audit information.
        courier.UpdatedAt = DateTime.UtcNow;
        courier.UpdatedBy = updatedBy;

        // Save changes.
        await _repository.UpdateAsync(courier);

        return Map(courier);
    }

    // Soft delete an existing courier.
    public async Task<bool> SoftDeleteAsync(int id, int? updatedBy)
    {
        // Find courier by ID.
        var courier = await _repository.GetByIdAsync(id);

        if (courier == null)
            return false;

        // Mark courier as deleted.
        courier.IsActive = false;
        courier.IsAvailable = false;
        courier.IsDeleted = true;

        // Update audit information.
        courier.UpdatedAt = DateTime.UtcNow;
        courier.UpdatedBy = updatedBy;

        // Save changes.
        await _repository.SoftDeleteAsync(courier);

        return true;
    }

    // Convert empty optional strings to null.
    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    // Map Courier entity to CourierResponseDto.
    private static CourierResponseDto Map(Courier c) => new()
    {
        CourierId = c.CourierId,
        UserId = c.UserId,
        EmployeeCode = c.EmployeeCode,
        FullName = c.FullName,
        Phone = c.Phone,
        Email = c.Email,
        VehicleType = c.VehicleType,
        VehicleNumber = c.VehicleNumber,
        IsAvailable = c.IsAvailable,
        IsActive = c.IsActive,
        CreatedAt = c.CreatedAt
    };
}
