
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.ParcelStatusDTO;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using CourierParcelTrackingSystem.API.Services.Interfaces;

namespace CourierParcelTrackingSystem.API.Services;

public class ParcelStatusService : IParcelStatusService
{
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
    public async Task<ParcelStatusResponseDto> CreateAsync(
        CreateParcelStatusRequestDto request,
        int? createdBy)
    {
        string statusName = request.StatusName.Trim();

        // Check duplicate status name.
        bool nameExists =
            await _parcelStatusRepository.StatusNameExistsAsync(statusName);

        if (nameExists)
        {
            throw new ArgumentException(
                "A parcel status with this name already exists.");
        }

        // Generate a unique status code.
        string statusCode = await GenerateStatusCodeAsync();

        var parcelStatus = new ParcelStatus
        {
            StatusCode = statusCode,
            StatusName = statusName,
            Description = Normalize(request.Description),
            DisplayOrder = request.DisplayOrder,
            IsTerminal = request.IsTerminal,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        await _parcelStatusRepository.AddAsync(parcelStatus);

        return MapToResponse(parcelStatus);
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
