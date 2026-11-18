using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.HubTransferDTO;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using CourierParcelTrackingSystem.API.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace CourierParcelTrackingSystem.API.Services;

public class HubTransferService : IHubTransferService
{
    private readonly IHubTransferRepository _repository;

    public HubTransferService(IHubTransferRepository repository) => _repository = repository;

    public async Task<PagedResponseDto<HubTransferResponseDto>> GetPagedAsync(
        PaginationRequestDto request,
        string? search,
        string? status)
    {
        if (!string.IsNullOrWhiteSpace(status))
        {
            status = status.Trim().ToUpperInvariant();
            if (status is not ("IN_TRANSIT" or "ARRIVED" or "CANCELLED"))
                throw new ArgumentException("Invalid hub transfer status.");
        }

        var result = await _repository.GetPagedAsync(request, search, status);
        return new PagedResponseDto<HubTransferResponseDto>
        {
            Items = result.Items.Select(Map).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalRecords = result.TotalRecords,
            TotalPages = result.TotalPages
        };
    }

    public async Task<HubTransferResponseDto?> GetByIdAsync(long transferId)
    {
        var transfer = await _repository.GetByIdAsync(transferId);
        return transfer is null ? null : Map(transfer);
    }

    public async Task<HubTransferOptionsDto> GetOptionsAsync()
    {
        var parcels = await _repository.GetEligibleParcelsAsync();
        var hubs = await _repository.GetActiveHubsAsync();

        return new HubTransferOptionsDto
        {
            Parcels = parcels.Select(p => new HubTransferParcelOptionDto
            {
                ParcelId = p.ParcelId,
                TrackingNumber = p.TrackingNumber,
                CurrentHubId = p.CurrentHubId ?? 0,
                CurrentHubName = p.CurrentHub?.HubName ?? string.Empty,
                CurrentStatus = p.CurrentStatus.StatusCode
            }).ToList(),
            Hubs = hubs.Select(h => new HubTransferHubOptionDto
            {
                HubId = h.HubId,
                HubCode = h.HubCode,
                HubName = h.HubName,
                City = h.City
            }).ToList()
        };
    }

    public async Task<HubTransferResponseDto> CreateAsync(
        CreateHubTransferRequestDto request,
        int createdBy)
    {
        var parcel = await _repository.GetParcelForTransferAsync(request.ParcelId);
        if (parcel is null)
            throw new KeyNotFoundException("Parcel not found or inactive.");

        if (parcel.DeliveredAt.HasValue || parcel.CurrentStatus.IsTerminal)
            throw new InvalidOperationException("Delivered or terminal parcels cannot be transferred.");

        if (await _repository.HasActiveTransferAsync(parcel.ParcelId))
            throw new InvalidOperationException("This parcel already has an active hub transfer.");

        if (!parcel.CurrentHubId.HasValue)
            throw new InvalidOperationException("The parcel's current hub could not be determined.");

        var destinationHub = await _repository.GetActiveHubAsync(request.DestinationHubId);
        if (destinationHub is null)
            throw new KeyNotFoundException("Destination hub not found or inactive.");

        if (parcel.CurrentHubId.Value == destinationHub.HubId)
            throw new InvalidOperationException("The destination hub must be different from the parcel's current hub.");

        var statusCode = parcel.CurrentStatus.StatusCode;
        if (statusCode is not ("AT_ORIGIN_HUB" or "PICKED_UP" or "IN_TRANSIT"))
            throw new InvalidOperationException($"Hub transfers cannot be created while the parcel is {statusCode}.");

        var inTransitStatus = await _repository.GetStatusByCodeAsync("IN_TRANSIT");
        if (inTransitStatus is null)
            throw new InvalidOperationException("The IN_TRANSIT parcel status is unavailable.");

        var now = DateTime.UtcNow;
        parcel.CurrentStatusId = inTransitStatus.ParcelStatusId;
        parcel.UpdatedAt = now;
        parcel.UpdatedBy = createdBy;

        var transfer = new HubTransfer
        {
            ParcelId = parcel.ParcelId,
            SourceHubId = parcel.CurrentHubId.Value,
            DestinationHubId = destinationHub.HubId,
            Status = "IN_TRANSIT",
            TransferredAt = now,
            Notes = Normalize(request.Notes),
            IsActive = true,
            IsDeleted = false,
            CreatedAt = now,
            CreatedBy = createdBy
        };

        await _repository.AddAsync(transfer);
        await _repository.AddStatusEventAsync(new StatusEvent
        {
            ParcelId = parcel.ParcelId,
            ParcelStatusId = inTransitStatus.ParcelStatusId,
            HubId = destinationHub.HubId,
            EventAt = now,
            Notes = Normalize(request.Notes)
                ?? $"Hub transfer created from {(parcel.CurrentHub?.HubName ?? "source hub")} to {destinationHub.HubName}.",
            CreatedAt = now,
            CreatedBy = createdBy
        });
        await _repository.AddAuditLogAsync(new AuditLog
        {
            ActorUserId = createdBy,
            Action = "CREATE",
            EntityName = "HubTransfer",
            EntityId = parcel.TrackingNumber,
            NewValues = $"{{\"parcelId\":{parcel.ParcelId},\"destinationHubId\":{destinationHub.HubId},\"status\":\"IN_TRANSIT\"}}",
            OccurredAt = now
        });
        await _repository.SaveChangesAsync();

        return await GetByIdAsync(transfer.HubTransferId)
            ?? throw new InvalidOperationException("The hub transfer could not be loaded after creation.");
    }

    public async Task<HubTransferResponseDto> ConfirmArrivalAsync(
        long transferId,
        ConfirmHubTransferArrivalRequestDto request,
        int updatedBy)
    {
        var transfer = await _repository.GetByIdAsync(transferId);
        if (transfer is null)
            throw new KeyNotFoundException("Hub transfer not found.");

        if (transfer.Status != "IN_TRANSIT" || !transfer.IsActive)
            throw new InvalidOperationException("Only an active IN_TRANSIT hub transfer can be confirmed.");

        var parcel = await _repository.GetParcelForTransferAsync(transfer.ParcelId);
        if (parcel is null)
            throw new KeyNotFoundException("Transferred parcel not found or inactive.");

        if (parcel.DeliveredAt.HasValue || parcel.CurrentStatus.IsTerminal)
            throw new InvalidOperationException("A delivered or terminal parcel cannot complete a hub transfer.");

        var destinationStatus = await _repository.GetStatusByCodeAsync("AT_DESTINATION_HUB");
        if (destinationStatus is null)
            throw new InvalidOperationException("The AT_DESTINATION_HUB parcel status is unavailable.");

        var now = DateTime.UtcNow;
        transfer.Status = "ARRIVED";
        transfer.ArrivedAt = now;
        transfer.IsActive = false;
        transfer.Notes = MergeNotes(transfer.Notes, request.Notes);
        transfer.UpdatedAt = now;
        transfer.UpdatedBy = updatedBy;

        parcel.CurrentHubId = transfer.DestinationHubId;
        parcel.CurrentStatusId = destinationStatus.ParcelStatusId;
        parcel.UpdatedAt = now;
        parcel.UpdatedBy = updatedBy;

        await _repository.AddStatusEventAsync(new StatusEvent
        {
            ParcelId = parcel.ParcelId,
            ParcelStatusId = destinationStatus.ParcelStatusId,
            HubId = transfer.DestinationHubId,
            EventAt = now,
            Notes = Normalize(request.Notes) ?? $"Parcel arrived at {transfer.DestinationHub?.HubName ?? "destination hub"}.",
            CreatedAt = now,
            CreatedBy = updatedBy
        });
        await _repository.AddAuditLogAsync(new AuditLog
        {
            ActorUserId = updatedBy,
            Action = "ARRIVED",
            EntityName = "HubTransfer",
            EntityId = transfer.HubTransferId.ToString(),
            NewValues = $"{{\"status\":\"ARRIVED\",\"currentHubId\":{transfer.DestinationHubId},\"parcelStatus\":\"AT_DESTINATION_HUB\"}}",
            OccurredAt = now
        });
        await _repository.SaveChangesAsync();
        return Map(transfer);
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? MergeNotes(string? existing, string? arrival)
    {
        var arrivalNote = Normalize(arrival);
        if (string.IsNullOrWhiteSpace(existing)) return arrivalNote;
        return string.IsNullOrWhiteSpace(arrivalNote)
            ? existing
            : $"{existing}\nArrival: {arrivalNote}";
    }

    private static HubTransferResponseDto Map(HubTransfer transfer) => new()
    {
        HubTransferId = transfer.HubTransferId,
        ParcelId = transfer.ParcelId,
        TrackingNumber = transfer.Parcel?.TrackingNumber ?? string.Empty,
        SourceHubId = transfer.SourceHubId,
        SourceHubName = transfer.SourceHub?.HubName ?? string.Empty,
        DestinationHubId = transfer.DestinationHubId,
        DestinationHubName = transfer.DestinationHub?.HubName ?? string.Empty,
        Status = transfer.Status,
        TransferredAt = transfer.TransferredAt,
        ArrivedAt = transfer.ArrivedAt,
        Notes = transfer.Notes,
        IsActive = transfer.IsActive
    };
}
