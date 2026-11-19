
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.ParcelStatusDTO;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using CourierParcelTrackingSystem.API.Services.Interfaces;

namespace CourierParcelTrackingSystem.API.Services;

public class ParcelStatusService : IParcelStatusService
{
    private static readonly HashSet<string> FixedStatusCodes = new(
        new[]
        {
            "BOOKED",
            "PICKED_UP",
            "AT_ORIGIN_HUB",
            "IN_TRANSIT",
            "AT_DESTINATION_HUB",
            "OUT_FOR_DELIVERY",
            "DELIVERY_ATTEMPTED",
            "DELIVERED",
            "ON_HOLD",
            "RETURNED",
            "CANCELLED"
        },
        StringComparer.OrdinalIgnoreCase);

    private readonly IParcelStatusRepository _parcelStatusRepository;

    public ParcelStatusService(
        IParcelStatusRepository parcelStatusRepository)
    {
        _parcelStatusRepository = parcelStatusRepository;
    }

    // Get paginated parcel statuses.
    public async Task<PagedResponseDto<ParcelStatusResponseDto>> GetPagedAsync(
        PaginationRequestDto pagination,
        string? search)
    {
        var result = await _parcelStatusRepository.GetPagedAsync(
            pagination, search);

        return new PagedResponseDto<ParcelStatusResponseDto>
        {
            Items = result.Items.Select(MapToResponse).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalRecords = result.TotalRecords,
            TotalPages = result.TotalPages
        };
    }

    // Get parcel status by ID.
    public async Task<ParcelStatusResponseDto?> GetByIdAsync(int id)
    {
        var parcelStatus = await _parcelStatusRepository.GetByIdAsync(id);

        return parcelStatus == null
            ? null
            : MapToResponse(parcelStatus);
    }

    // Create a new parcel status.
    public Task<ParcelStatusResponseDto> CreateAsync(
        CreateParcelStatusRequestDto request,
        int? createdBy)
    {
        return Task.FromException<ParcelStatusResponseDto>(
            new InvalidOperationException(
                "The parcel status catalog is fixed and must reuse existing database status records."));
    }

    // Update an existing parcel status.
    public async Task<ParcelStatusResponseDto?> UpdateAsync(
        int id,
        UpdateParcelStatusRequestDto request,
        int? updatedBy)
    {
        var parcelStatus =
            await _parcelStatusRepository.GetByIdAsync(id);

        if (parcelStatus == null)
        {
            return null;
        }

        if (FixedStatusCodes.Contains(parcelStatus.StatusCode) &&
            (!request.IsActive || request.IsTerminal != parcelStatus.IsTerminal))
        {
            throw new InvalidOperationException(
                "The workflow status code, active state, and terminal state must not be changed.");
        }

        string statusName = request.StatusName.Trim();

        // Check duplicate name excluding the current record.
        bool nameExists =
            await _parcelStatusRepository.StatusNameExistsAsync(
                statusName, id);

        if (nameExists)
        {
            throw new ArgumentException(
                "A parcel status with this name already exists.");
        }

        parcelStatus.StatusName = statusName;
        parcelStatus.Description = Normalize(request.Description);
        parcelStatus.DisplayOrder = request.DisplayOrder;
        parcelStatus.IsTerminal = request.IsTerminal;
        parcelStatus.IsActive = request.IsActive;
        parcelStatus.UpdatedAt = DateTime.UtcNow;
        parcelStatus.UpdatedBy = updatedBy;

        await _parcelStatusRepository.UpdateAsync(parcelStatus);

        return MapToResponse(parcelStatus);
    }

    // Soft delete a parcel status.
    public async Task<bool> SoftDeleteAsync(int id, int? updatedBy)
    {
        var parcelStatus =
            await _parcelStatusRepository.GetByIdAsync(id);

        if (parcelStatus == null)
        {
            return false;
        }

        if (FixedStatusCodes.Contains(parcelStatus.StatusCode))
        {
            throw new InvalidOperationException(
                "Existing parcel workflow statuses cannot be soft deleted.");
        }

        parcelStatus.IsActive = false;
        parcelStatus.IsDeleted = true;
        parcelStatus.UpdatedAt = DateTime.UtcNow;
        parcelStatus.UpdatedBy = updatedBy;

        await _parcelStatusRepository.SoftDeleteAsync(parcelStatus);

        return true;
    }

    // Generate a unique status code.
    private async Task<string> GenerateStatusCodeAsync()
    {
        string statusCode;

        do
        {
            statusCode =
                $"STS-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

        } while (await _parcelStatusRepository.StatusCodeExistsAsync(
            statusCode));

        return statusCode;
    }

    // Convert empty optional strings to null.
    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    // Map ParcelStatus entity to response DTO.
    private static ParcelStatusResponseDto MapToResponse(
        ParcelStatus parcelStatus)
    {
        return new ParcelStatusResponseDto
        {
            ParcelStatusId = parcelStatus.ParcelStatusId,
            StatusCode = parcelStatus.StatusCode,
            StatusName = parcelStatus.StatusName,
            Description = parcelStatus.Description,
            DisplayOrder = parcelStatus.DisplayOrder,
            IsTerminal = parcelStatus.IsTerminal,
            IsActive = parcelStatus.IsActive,
            CreatedAt = parcelStatus.CreatedAt
        };
    }
}
