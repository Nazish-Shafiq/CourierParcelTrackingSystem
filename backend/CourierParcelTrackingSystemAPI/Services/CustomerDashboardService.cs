using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.CustomerDashboardDTO;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using CourierParcelTrackingSystem.API.Services.Interfaces;

namespace CourierParcelTrackingSystem.API.Services;

public class CustomerDashboardService : ICustomerDashboardService
{
    private readonly IParcelRepository _repository;

    public CustomerDashboardService(IParcelRepository repository)
    {
        _repository = repository;
    }

    public async Task<CustomerDashboardSummaryDto> GetSummaryAsync(int userId)
    {
        var counts = await _repository.GetCustomerParcelCountsAsync(userId);
        return new CustomerDashboardSummaryDto
        {
            TotalParcels = counts.TotalParcels,
            BookedParcels = counts.BookedParcels,
            DeliveredParcels = counts.DeliveredParcels
        };
    }

    public async Task<PagedResponseDto<CustomerParcelDto>> GetParcelsAsync(
        int userId,
        PaginationRequestDto pagination,
        string? search,
        string? statusCode)
    {
        var result = await _repository.GetCustomerParcelsAsync(
            userId,
            pagination,
            search,
            statusCode);

        return new PagedResponseDto<CustomerParcelDto>
        {
            Items = result.Items.Select(MapParcel).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalRecords = result.TotalRecords,
            TotalPages = result.TotalPages
        };
    }

    public async Task<CustomerParcelDetailsDto?> GetParcelDetailsAsync(int userId, long parcelId)
    {
        var parcel = await _repository.GetCustomerParcelDetailsAsync(userId, parcelId);
        return parcel is null ? null : MapDetails(parcel);
    }

    public async Task<CustomerParcelDetailsDto?> GetParcelByTrackingNumberAsync(
        int userId,
        string trackingNumber)
    {
        if (string.IsNullOrWhiteSpace(trackingNumber))
            return null;

        var parcel = await _repository.GetCustomerParcelByTrackingNumberAsync(
            userId,
            trackingNumber.Trim());
        return parcel is null ? null : MapDetails(parcel);
    }

    private static CustomerParcelDto MapParcel(Parcel parcel) => new()
    {
        ParcelId = parcel.ParcelId,
        TrackingNumber = parcel.TrackingNumber,
        BookingDate = parcel.BookingDate,
        OriginHubName = parcel.Route?.OriginHub?.HubName,
        DestinationHubName = parcel.Route?.DestinationHub?.HubName,
        TotalWeightKg = parcel.TotalWeightKg,
        CurrentStatusName = parcel.CurrentStatus.StatusName,
        CurrentStatusCode = parcel.CurrentStatus.StatusCode,
        ExpectedDeliveryDate = parcel.ExpectedDeliveryDate
    };

    private static CustomerParcelDetailsDto MapDetails(Parcel parcel) => new()
    {
        ParcelId = parcel.ParcelId,
        TrackingNumber = parcel.TrackingNumber,
        BookingDate = parcel.BookingDate,
        ExpectedDeliveryDate = parcel.ExpectedDeliveryDate,
        DeliveredAt = parcel.DeliveredAt,
        TotalWeightKg = parcel.TotalWeightKg,
        DeclaredValue = parcel.DeclaredValue,
        SpecialInstructions = parcel.SpecialInstructions,
        CurrentStatusName = parcel.CurrentStatus.StatusName,
        CurrentStatusCode = parcel.CurrentStatus.StatusCode,
        CurrentStatusDescription = parcel.CurrentStatus.Description,
        OriginHubName = parcel.Route?.OriginHub?.HubName,
        DestinationHubName = parcel.Route?.DestinationHub?.HubName,
        CurrentHubName = parcel.CurrentHub?.HubName,
        Sender = new CustomerContactDto
        {
            Name = parcel.SenderCustomer.FullName,
            Phone = parcel.SenderCustomer.Phone,
            Email = parcel.SenderCustomer.Email,
            AddressLine = parcel.SenderCustomer.AddressLine,
            City = parcel.SenderCustomer.City,
            Province = parcel.SenderCustomer.Province,
            PostalCode = parcel.SenderCustomer.PostalCode
        },
        Recipient = parcel.ParcelRecipient is { IsActive: true, IsDeleted: false } recipient
            ? new CustomerContactDto
            {
                Name = recipient.RecipientName,
                Phone = recipient.Phone,
                Email = recipient.Email,
                AddressLine = recipient.AddressLine,
                City = recipient.City,
                Province = recipient.Province,
                PostalCode = recipient.PostalCode
            }
            : null,
        TrackingHistory = parcel.StatusEvents
            .OrderBy(statusEvent => statusEvent.EventAt)
            .ThenBy(statusEvent => statusEvent.StatusEventId)
            .Select(statusEvent => new CustomerTrackingEventDto
            {
                StatusEventId = statusEvent.StatusEventId,
                StatusName = statusEvent.ParcelStatus.StatusName,
                StatusCode = statusEvent.ParcelStatus.StatusCode,
                Description = statusEvent.ParcelStatus.Description,
                HubName = statusEvent.Hub?.HubName,
                OriginHubName = parcel.Route?.OriginHub?.HubName,
                DestinationHubName = parcel.Route?.DestinationHub?.HubName,
                EventAt = statusEvent.EventAt
            })
            .ToList()
    };
}
