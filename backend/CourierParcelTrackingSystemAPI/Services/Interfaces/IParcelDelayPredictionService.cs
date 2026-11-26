using CourierParcelTrackingSystem.API.DTOs.ParcelDelayPredictionDTO;

namespace CourierParcelTrackingSystem.API.Services.Interfaces;

public interface IParcelDelayPredictionService
{
    Task<ParcelDelayModelStatusDto> GetAdminStatusAsync();
    Task<ParcelDelayModelStatusDto> TrainAsync(int requestedBy);
    Task<ParcelDelayPredictionsResponseDto> GetDispatcherPredictionsAsync(int requestedBy);
}
