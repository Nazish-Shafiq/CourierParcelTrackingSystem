
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.ParcelBookingDTO;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using CourierParcelTrackingSystem.API.Services.Interfaces;

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
        GetPagedAsync(PaginationRequestDto pagination, string? search)
    {
        var result = await _repository.GetPagedAsync(pagination, search);

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
        int? createdBy)
    {
        // Validate sender customer.
        bool customerExists =
            await _repository.CustomerExistsAsync(request.SenderCustomerId);

        if (!customerExists)
        {
            throw new ArgumentException(
                "Selected sender customer does not exist or is inactive.");
        }

        // Validate route when supplied.
        if (request.RouteId.HasValue)
        {
            bool routeExists =
                await _repository.RouteExistsAsync(request.RouteId.Value);

            if (!routeExists)
            {
                throw new ArgumentException(
                    "Selected route does not exist or is inactive.");
            }
        }

        // Find the initial BOOKED status.
        var bookedStatus = await _repository.GetBookedStatusAsync();

        if (bookedStatus == null)
        {
            throw new InvalidOperationException(
                "The active BOOKED parcel status was not found. Please configure it first.");
        }

        // Generate a unique tracking number.
        string trackingNumber;

        do
        {
            trackingNumber = GenerateTrackingNumber();

        } while (await _repository.TrackingNumberExistsAsync(trackingNumber));

        DateTime now = DateTime.UtcNow;

        // Create Parcel entity.
        var parcel = new Parcel
        {
            TrackingNumber = trackingNumber,
            SenderCustomerId = request.SenderCustomerId,
            RouteId = request.RouteId,
            CurrentStatusId = bookedStatus.ParcelStatusId,
            BookingDate = now,
            ExpectedDeliveryDate = request.ExpectedDeliveryDate,
            DeliveredAt = null,
            TotalWeightKg = request.TotalWeightKg,
            DeclaredValue = request.DeclaredValue,
            SpecialInstructions = Normalize(request.SpecialInstructions),
            IsActive = true,
            IsDeleted = false,
            CreatedAt = now,
            CreatedBy = createdBy
        };

        // Create recipient entity.
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

        // Save parcel and recipient together.
        await _repository.AddBookingAsync(parcel, recipient);

        // Reload the saved booking with its navigation properties.
        var savedParcel = await _repository.GetByIdAsync(parcel.ParcelId);

        if (savedParcel == null)
        {
            throw new InvalidOperationException(
                "Parcel booking was created but could not be retrieved.");
        }

        return MapToResponse(savedParcel);
    }

    // Generate a tracking number.
    private static string GenerateTrackingNumber()
    {
        return $"CPT-{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}";
    }

    // Convert empty strings into null.
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
