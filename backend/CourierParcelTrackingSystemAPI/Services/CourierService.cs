
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.CouriersDTO;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using CourierParcelTrackingSystem.API.Services.Interfaces;

namespace CourierParcelTrackingSystem.API.Services;

public class CourierService : ICourierService
{
    private readonly ICourierRepository _repository;
    private readonly IParcelAssignmentRepository _assignmentRepository;
    private readonly IHubRepository _hubRepository;

    public CourierService(
        ICourierRepository repository,
        IParcelAssignmentRepository assignmentRepository,
        IHubRepository hubRepository)
    {
        _repository = repository;
        _assignmentRepository = assignmentRepository;
        _hubRepository = hubRepository;
    }

    // Get couriers with pagination and optional search.
    public async Task<PagedResponseDto<CourierResponseDto>> GetPagedAsync(
        PaginationRequestDto pagination,
        string? search,
        bool? isAvailable,
        bool? isActive)
    {
        var result = await _repository.GetPagedAsync(
            pagination, search, isAvailable, isActive);

        return new PagedResponseDto<CourierResponseDto>
        {
            Items = result.Items.Select(Map).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalRecords = result.TotalRecords,
            TotalPages = result.TotalPages
        };
    }

    public async Task<List<CourierDispatchOptionDto>> GetDispatchOptionsAsync(long parcelId)
    {
        var parcel = await _assignmentRepository.GetParcelForAssignmentAsync(parcelId);
        if (parcel is null)
            throw new KeyNotFoundException("Parcel not found or inactive.");

        ParcelAssignmentHubResolver.EnsureParcelCanBeAssigned(parcel);
        var operationalHub = ParcelAssignmentHubResolver.ResolveOperationalHub(parcel);
        if (operationalHub is null || !operationalHub.IsActive || operationalHub.IsDeleted)
            throw new InvalidOperationException(
                "The parcel does not have an active operational hub for courier assignment.");

        var couriers = await _repository.GetActiveCouriersForDispatchAsync(
            operationalHub.HubId);

        return couriers.Select(courier => new CourierDispatchOptionDto
        {
            CourierId = courier.CourierId,
            HubId = operationalHub.HubId,
            HubName = operationalHub.HubName,
            EmployeeCode = courier.EmployeeCode,
            FullName = courier.FullName,
            VehicleType = courier.VehicleType,
            IsAvailable = courier.IsAvailable
        }).ToList();
    }

    // Get courier by ID.
    public async Task<CourierResponseDto?> GetByIdAsync(int id)
    {
        var courier = await _repository.GetByIdAsync(id);

        return courier is null ? null : Map(courier);
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

        if (!request.HubId.HasValue)
            throw new ArgumentException(
                "An active hub must be selected for a Courier profile.",
                nameof(request.HubId));

        var assignedHub = await _hubRepository.GetByIdAsync(request.HubId.Value);
        if (assignedHub is null || !assignedHub.IsActive)
            throw new ArgumentException(
                "The selected hub does not exist or is inactive.",
                nameof(request.HubId));

        if (courier.HubId != assignedHub.HubId &&
            await _repository.HasActiveAssignmentAsync(id))
        {
            throw new InvalidOperationException(
                "A Courier's hub cannot be changed while they have an active parcel assignment.");
        }

        if (request.IsActive && courier.User?.IsDeleted == true)
        {
            throw new InvalidOperationException(
                "A deleted linked user account cannot be reactivated through Courier management.");
        }

        if (request.UserId.HasValue)
        {
            await EnsureUserCanBeLinkedAsync(request.UserId.Value, id);
            courier.UserId = request.UserId.Value;
        }

        // Update courier details.
        courier.FullName = request.FullName.Trim();
        courier.Phone = request.Phone.Trim();
        courier.Email = Normalize(request.Email);
        courier.HubId = assignedHub.HubId;
        courier.Hub = assignedHub;
        courier.VehicleType = Normalize(request.VehicleType);
        courier.VehicleNumber = Normalize(request.VehicleNumber);
        var courierStatusChanged = courier.IsActive != request.IsActive;
        courier.IsAvailable = request.IsActive && request.IsAvailable;
        courier.IsActive = request.IsActive;
        if (courierStatusChanged && courier.User != null)
        {
            courier.User.IsActive = request.IsActive;
            courier.User.UpdatedAt = DateTime.UtcNow;
            courier.User.UpdatedBy = updatedBy;
        }

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

        if (await _repository.HasActiveAssignmentAsync(id))
        {
            throw new InvalidOperationException(
                "A Courier with an active parcel assignment cannot be deactivated.");
        }

        // Mark courier as deleted.
        courier.IsActive = false;
        courier.IsAvailable = false;
        courier.IsDeleted = true;
        if (courier.User != null)
        {
            courier.User.IsActive = false;
            courier.User.UpdatedAt = DateTime.UtcNow;
            courier.User.UpdatedBy = updatedBy;
        }

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

    private async Task EnsureUserCanBeLinkedAsync(int userId, int? excludeCourierId = null)
    {
        if (!await _repository.IsActiveCourierUserAsync(userId))
            throw new ArgumentException("The linked user must be an active account with the Courier role.");

        if (await _repository.UserHasCourierProfileAsync(userId, excludeCourierId))
            throw new InvalidOperationException("This user already has a Courier profile.");
    }

    // Map Courier entity to CourierResponseDto.
    private static CourierResponseDto Map(Courier c) => new()
    {
        CourierId = c.CourierId,
        UserId = c.UserId,
        HubId = c.HubId,
        HubName = c.Hub?.HubName,
        HubCode = c.Hub?.HubCode,
        HubCity = c.Hub?.City,
        HubProvince = c.Hub?.Province,
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
