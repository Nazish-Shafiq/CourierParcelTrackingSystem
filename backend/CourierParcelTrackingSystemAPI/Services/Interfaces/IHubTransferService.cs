using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.HubTransferDTO;

namespace CourierParcelTrackingSystem.API.Services.Interfaces;

public interface IHubTransferService
{
    Task<PagedResponseDto<HubTransferResponseDto>> GetPagedAsync(PaginationRequestDto request, string? search, string? status);
    Task<HubTransferResponseDto?> GetByIdAsync(long transferId);
    Task<HubTransferOptionsDto> GetOptionsAsync();
    Task<HubTransferResponseDto> CreateAsync(CreateHubTransferRequestDto request, int createdBy);
    Task<HubTransferResponseDto> ConfirmArrivalAsync(long transferId, ConfirmHubTransferArrivalRequestDto request, int updatedBy);
}
