
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.ParcelBookingDTO;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using CourierParcelTrackingSystem.API.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using RouteEntity = CourierParcelTrackingSystem.API.Entities.Route;

namespace CourierParcelTrackingSystem.API.Services;

public class ParcelBookingService : IParcelBookingService
{
    private readonly IParcelBookingRepository _repository;

    public ParcelBookingService(IParcelBookingRepository repository)
    {
        _repository = repository;
    }

    // Retrieve paginated parcel bookings.
    public async Task<PagedResponseDto<ParcelBookingResponseDto>>
        GetPagedAsync(
            PaginationRequestDto pagination,
            string? search,
            string? filter,
            DateTime? fromBookingDate,
            DateTime? beforeBookingDate)
    {
        var result = await _repository.GetPagedAsync(
            pagination,
            search,
            filter,
            fromBookingDate,
            beforeBookingDate);

        return new PagedResponseDto<ParcelBookingResponseDto>
        {
            Items = result.Items.Select(MapToResponse).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalRecords = result.TotalRecords,
            TotalPages = result.TotalPages
        };
    }

    // Retrieve booking details by parcel ID.
    public async Task<ParcelBookingResponseDto?> GetByIdAsync(long id)
    {
        var parcel = await _repository.GetByIdAsync(id);

        return parcel == null ? null : MapToResponse(parcel);
    }

    // Create a new parcel booking.
    public async Task<ParcelBookingResponseDto> CreateBookingAsync(
        CreateParcelBookingRequestDto request,
        int? createdBy,
        string? idempotencyKey)
    {
        var normalizedIdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey)
            ? null
            : idempotencyKey.Trim();
        var trackingNumber = GenerateTrackingNumber(
            normalizedIdempotencyKey,
            createdBy);

        if (normalizedIdempotencyKey is not null)
        {
            var existingBooking = await _repository.GetByTrackingNumberAsync(
                trackingNumber);
            if (existingBooking is not null)
            {
                if (!MatchesBookingRequest(
                    existingBooking,
                    request,
                    createdBy))
                {
                    throw new InvalidOperationException(
                        "This idempotency key was already used for a different parcel booking.");
                }

                return MapToResponse(existingBooking);
            }
        }
        else
        {
            while (await _repository.TrackingNumberExistsAsync(trackingNumber))
                trackingNumber = GenerateTrackingNumber(null, createdBy);
        }

        var senderCustomer = await _repository.GetActiveCustomerAsync(
            request.SenderCustomerId);
        if (senderCustomer is null)
        {
            throw new ArgumentException(
                "Selected sender customer does not exist or is inactive.");
        }

        RouteEntity? route = null;
        if (request.RouteId.HasValue)
        {
            route = await _repository.GetActiveRouteAsync(request.RouteId.Value);
            if (route is null)
            {
                throw new ArgumentException(
                    "Selected route does not exist or is inactive.");
            }
        }

        var bookedStatus = await _repository.GetBookedStatusAsync();
        if (bookedStatus == null)
        {
            throw new InvalidOperationException(
                "The active BOOKED parcel status was not found. Please configure it first.");
        }

        var now = DateTime.UtcNow;
        var normalizedExpectedDeliveryDate = TruncateToSeconds(
            request.ExpectedDeliveryDate);
        var normalizedWeight = decimal.Round(
            request.TotalWeightKg,
            3,
            MidpointRounding.AwayFromZero);
        decimal? normalizedDeclaredValue = request.DeclaredValue.HasValue
            ? decimal.Round(
                request.DeclaredValue.Value,
                2,
                MidpointRounding.AwayFromZero)
            : (decimal?)null;

        var parcel = new Parcel
        {
            TrackingNumber = trackingNumber,
            SenderCustomerId = request.SenderCustomerId,
            SenderCustomer = senderCustomer,
            RouteId = request.RouteId,
            Route = route,
            CurrentStatusId = bookedStatus.ParcelStatusId,
            CurrentStatus = bookedStatus,
            BookingDate = now,
            ExpectedDeliveryDate = normalizedExpectedDeliveryDate,
            DeliveredAt = null,
            TotalWeightKg = normalizedWeight,
            DeclaredValue = normalizedDeclaredValue,
            SpecialInstructions = Normalize(request.SpecialInstructions),
            IsActive = true,
            IsDeleted = false,
            CreatedAt = now,
            CreatedBy = createdBy
        };

        var recipient = new ParcelRecipient
        {
            RecipientName = request.RecipientName.Trim(),
            Phone = request.RecipientPhone.Trim(),
            Email = Normalize(request.RecipientEmail),
            AddressLine = request.RecipientAddressLine.Trim(),
            City = request.RecipientCity.Trim(),
            Province = Normalize(request.RecipientProvince),
            PostalCode = Normalize(request.RecipientPostalCode),
            CreatedAt = now,
            CreatedBy = createdBy,
            IsActive = true,
            IsDeleted = false
        };
        parcel.ParcelRecipient = recipient;

        parcel.StatusEvents.Add(new StatusEvent
        {
            ParcelStatusId = bookedStatus.ParcelStatusId,
            ParcelStatus = bookedStatus,
            EventAt = now,
            CreatedAt = now,
            CreatedBy = createdBy
        });

        var savedParcel = await _repository.AddBookingAsync(
            parcel,
            recipient,
            normalizedIdempotencyKey is not null);
        if (normalizedIdempotencyKey is not null &&
            !MatchesBookingRequest(savedParcel, request, createdBy))
        {
            throw new InvalidOperationException(
                "This idempotency key was already used for a different parcel booking.");
        }

        return MapToResponse(savedParcel);
    }

    private static string GenerateTrackingNumber(
        string? idempotencyKey,
        int? createdBy)
    {
        if (idempotencyKey is null)
            return $"CPT-{Guid.NewGuid():N}".ToUpperInvariant();

        if (!Guid.TryParseExact(idempotencyKey, "D", out var requestId))
            throw new ArgumentException(
                "The Idempotency-Key header must contain a UUID.");

        var scopedKey = $"{createdBy?.ToString() ?? "anonymous"}:{requestId:N}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(scopedKey));
        return $"CPT-{Convert.ToHexString(hash.AsSpan(0, 16))}";
    }

    private static bool MatchesBookingRequest(
        Parcel parcel,
        CreateParcelBookingRequestDto request,
        int? createdBy)
    {
        var recipient = parcel.ParcelRecipient;
        return parcel.CreatedBy == createdBy &&
            parcel.SenderCustomerId == request.SenderCustomerId &&
            parcel.RouteId == request.RouteId &&
            parcel.TotalWeightKg == decimal.Round(
                request.TotalWeightKg,
                3,
                MidpointRounding.AwayFromZero) &&
            parcel.DeclaredValue == (request.DeclaredValue.HasValue
                ? decimal.Round(
                    request.DeclaredValue.Value,
                    2,
                    MidpointRounding.AwayFromZero)
                : (decimal?)null) &&
            parcel.ExpectedDeliveryDate == TruncateToSeconds(
                request.ExpectedDeliveryDate) &&
            parcel.SpecialInstructions == Normalize(request.SpecialInstructions) &&
            recipient is not null &&
            recipient.RecipientName == request.RecipientName.Trim() &&
            recipient.Phone == request.RecipientPhone.Trim() &&
            recipient.Email == Normalize(request.RecipientEmail) &&
            recipient.AddressLine == request.RecipientAddressLine.Trim() &&
            recipient.City == request.RecipientCity.Trim() &&
            recipient.Province == Normalize(request.RecipientProvince) &&
            recipient.PostalCode == Normalize(request.RecipientPostalCode);
    }

    private static DateTime? TruncateToSeconds(DateTime? value)
    {
        if (!value.HasValue)
            return null;

        var dateTime = value.Value;
        var ticks = dateTime.Ticks - dateTime.Ticks % TimeSpan.TicksPerSecond;
        return new DateTime(ticks, dateTime.Kind);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    // Map Parcel entity to response DTO.
    private static ParcelBookingResponseDto MapToResponse(Parcel parcel)
    {
        return new ParcelBookingResponseDto
        {
            ParcelId = parcel.ParcelId,
            TrackingNumber = parcel.TrackingNumber,

            SenderCustomerId = parcel.SenderCustomerId,
            SenderCustomerName = parcel.SenderCustomer.FullName,

            RouteId = parcel.RouteId,
            RouteName = parcel.Route?.RouteName,

            CurrentStatusId = parcel.CurrentStatusId,
            CurrentStatusName = parcel.CurrentStatus.StatusName,

            BookingDate = parcel.BookingDate,
            ExpectedDeliveryDate = parcel.ExpectedDeliveryDate,
            DeliveredAt = parcel.DeliveredAt,

            TotalWeightKg = parcel.TotalWeightKg,
            DeclaredValue = parcel.DeclaredValue,
            SpecialInstructions = parcel.SpecialInstructions,

            IsActive = parcel.IsActive,

            Recipient = parcel.ParcelRecipient == null
                ? null
                : new ParcelRecipientResponseDto
                {
                    ParcelRecipientId =
                        parcel.ParcelRecipient.ParcelRecipientId,

                    RecipientName =
                        parcel.ParcelRecipient.RecipientName,

                    Phone = parcel.ParcelRecipient.Phone,
                    Email = parcel.ParcelRecipient.Email,

                    AddressLine =
                        parcel.ParcelRecipient.AddressLine,

                    City = parcel.ParcelRecipient.City,
                    Province = parcel.ParcelRecipient.Province,
                    PostalCode = parcel.ParcelRecipient.PostalCode
                }
        };
    }
}
