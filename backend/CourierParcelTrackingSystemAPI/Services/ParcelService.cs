using CourierParcelTrackingSystem.API.DTOs.ParcelDTO;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using CourierParcelTrackingSystem.API.Services.Interfaces;

namespace CourierParcelTrackingSystem.API.Services;

public class ParcelService : IParcelService
{
    private readonly IParcelRepository _repository;
    public ParcelService(IParcelRepository repository) => _repository = repository;

    public async Task<List<ParcelResponseDto>> GetAllAsync() =>
        (await _repository.GetAllAsync()).Select(Map).ToList();

    public async Task<ParcelResponseDto?> GetByIdAsync(long id)
    {
        var parcel = await _repository.GetByIdAsync(id);
        return parcel is null ? null : Map(parcel);
    }

    public async Task<ParcelResponseDto> CreateAsync(
        CreateParcelRequestDto request,
        int createdBy)
    {
        if (request.ExpectedDeliveryDate.HasValue &&
            request.ExpectedDeliveryDate.Value < DateTime.UtcNow.Date)
            throw new ArgumentException("Expected delivery date cannot be in the past.");

        if (!await _repository.CustomerExistsAsync(request.SenderCustomerId))
            throw new KeyNotFoundException("The selected sender customer does not exist or is inactive.");

        if (request.RouteId.HasValue && !await _repository.RouteExistsAsync(request.RouteId.Value))
            throw new KeyNotFoundException("The selected route does not exist or is inactive.");

        if (!await _repository.StatusExistsAsync(request.CurrentStatusId))
            throw new KeyNotFoundException("The selected parcel status does not exist or is inactive.");

        var now = DateTime.UtcNow;
        var parcel = new Parcel
        {
            TrackingNumber = $"CP{now:yyyyMMddHHmmss}{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            SenderCustomerId = request.SenderCustomerId,
            RouteId = request.RouteId,
            CurrentStatusId = request.CurrentStatusId,
            BookingDate = now,
            ExpectedDeliveryDate = request.ExpectedDeliveryDate,
            TotalWeightKg = request.TotalWeightKg,
            DeclaredValue = request.DeclaredValue,
            SpecialInstructions = request.SpecialInstructions,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = now,
            CreatedBy = createdBy
        };

        return Map(await _repository.CreateAsync(parcel));
    }

    private static ParcelResponseDto Map(Parcel p) => new()
    {
        ParcelId = p.ParcelId, TrackingNumber = p.TrackingNumber,
        SenderCustomerId = p.SenderCustomerId, SenderName = p.SenderCustomer.FullName,
        RouteId = p.RouteId, RouteName = p.Route?.RouteName,
        CurrentStatusId = p.CurrentStatusId, CurrentStatus = p.CurrentStatus.StatusName,
        BookingDate = p.BookingDate, ExpectedDeliveryDate = p.ExpectedDeliveryDate,
        DeliveredAt = p.DeliveredAt ?? p.StatusEvents
            .OrderByDescending(statusEvent => statusEvent.EventAt)
            .ThenByDescending(statusEvent => statusEvent.StatusEventId)
            .Select(statusEvent => (DateTime?)statusEvent.EventAt)
            .FirstOrDefault(),
        TotalWeightKg = p.TotalWeightKg,
        DeclaredValue = p.DeclaredValue, SpecialInstructions = p.SpecialInstructions
    };
}
