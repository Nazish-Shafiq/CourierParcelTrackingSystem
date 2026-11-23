using CourierParcelTrackingSystem.API.DTOs.ParcelDTO;

namespace CourierParcelTrackingSystem.API.Services.Interfaces;

public interface IParcelService
{
    Task<List<ParcelResponseDto>> GetAllAsync();
    Task<ParcelResponseDto?> GetByIdAsync(long id);
    Task<ParcelResponseDto> CreateAsync(CreateParcelRequestDto request, int createdBy);
}
