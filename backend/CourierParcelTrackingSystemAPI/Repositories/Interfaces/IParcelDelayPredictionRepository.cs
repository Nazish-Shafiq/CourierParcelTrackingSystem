using CourierParcelTrackingSystem.API.Entities;

namespace CourierParcelTrackingSystem.API.Repositories.Interfaces;

public interface IParcelDelayPredictionRepository
{
    Task<List<Parcel>> GetCompletedParcelsForTrainingAsync();
    Task<List<Parcel>> GetEligibleParcelsForPredictionAsync();
    Task<(AiModel Model, AiPrompt Prompt)> GetOrCreateModelReferencesAsync(int requestedBy);
    Task<AiModel?> GetModelAsync();
    Task<AiPrompt?> GetPromptAsync();
    Task RecordSuccessfulTrainingAsync(
        int modelId,
        string description,
        int updatedBy,
        AiRequest request);
    Task AddRequestsAsync(IReadOnlyCollection<AiRequest> requests);
    Task<int> GetCompletedPredictionCountAsync();
    Task<AiRequest?> GetLatestSuccessfulTrainingRequestAsync();
    Task<AiRequest?> GetLatestTrainingAttemptAsync();
}
