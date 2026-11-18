
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.HubDTO;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using CourierParcelTrackingSystem.API.Services.Interfaces;

namespace CourierParcelTrackingSystem.API.Services;

public class HubService : IHubService
{
    private readonly IHubRepository _repository;

    public HubService(IHubRepository repository)
    {
        _repository = repository;
    }

    // Get hubs with pagination and optional search.
    public async Task<PagedResponseDto<HubResponseDto>> GetPagedAsync(
        PaginationRequestDto pagination,
        string? search,
        bool? activeOnly)
    {
        var result = await _repository.GetPagedAsync(pagination, search, activeOnly);

        return new PagedResponseDto<HubResponseDto>
        {
            Items = result.Items.Select(Map).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalRecords = result.TotalRecords,
            TotalPages = result.TotalPages
        };
    }

    // Get hub by ID.
    public async Task<HubResponseDto?> GetByIdAsync(int id)
    {
        var hub = await _repository.GetByIdAsync(id);

        return hub is null ? null : Map(hub);
    }

    // Create a new hub.
    public async Task<HubResponseDto> CreateAsync(
        CreateHubRequestDto request,
        int? createdBy)
    {
        // Generate a unique hub code.
        string hubCode;

        do
        {
            hubCode =
                $"HUB-{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}";
        }
        while (await _repository.HubCodeExistsAsync(hubCode));

        // Create hub entity.
        var hub = new Hub
        {
            HubCode = hubCode,
            HubName = request.HubName.Trim(),
            AddressLine = request.AddressLine.Trim(),
            City = request.City.Trim(),
            Province = Normalize(request.Province),
            PostalCode = Normalize(request.PostalCode),
            Country = request.Country.Trim(),
            ContactPhone = Normalize(request.ContactPhone),

            // Default values.
            IsActive = true,
            IsDeleted = false,

            // Audit information.
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        // Save hub in database.
        await _repository.AddAsync(hub);

        return Map(hub);
    }

    // Update an existing hub.
    public async Task<HubResponseDto?> UpdateAsync(
        int id,
        UpdateHubRequestDto request,
        int? updatedBy)
    {
        // Find hub by ID.
        var hub = await _repository.GetByIdAsync(id);

        if (hub == null)
            return null;

        // Update hub details.
        hub.HubName = request.HubName.Trim();
        hub.AddressLine = request.AddressLine.Trim();
        hub.City = request.City.Trim();
        hub.Province = Normalize(request.Province);
        hub.PostalCode = Normalize(request.PostalCode);
        hub.Country = request.Country.Trim();
        hub.ContactPhone = Normalize(request.ContactPhone);
        hub.IsActive = request.IsActive;

        // Update audit information.
        hub.UpdatedAt = DateTime.UtcNow;
        hub.UpdatedBy = updatedBy;

        await _repository.UpdateAsync(hub);

        return Map(hub);
    }

    // Soft delete a hub.
    public async Task<bool> SoftDeleteAsync(int id, int? updatedBy)
    {
        // Find hub by ID.
        var hub = await _repository.GetByIdAsync(id);

        if (hub == null)
            return false;

        // Mark hub as deleted.
        hub.IsActive = false;
        hub.IsDeleted = true;

        // Update audit information.
        hub.UpdatedAt = DateTime.UtcNow;
        hub.UpdatedBy = updatedBy;

        await _repository.SoftDeleteAsync(hub);

        return true;
    }

    // Convert empty optional strings to null.
    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    // Map Hub entity to HubResponseDto.
    private static HubResponseDto Map(Hub h) => new()
    {
        HubId = h.HubId,
        HubCode = h.HubCode,
        HubName = h.HubName,
        AddressLine = h.AddressLine,
        City = h.City,
        Province = h.Province,
        PostalCode = h.PostalCode,
        Country = h.Country,
        ContactPhone = h.ContactPhone,
        IsActive = h.IsActive,
        CreatedAt = h.CreatedAt
    };
}
