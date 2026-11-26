using CourierParcelTrackingSystem.API.Data;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourierParcelTrackingSystem.API.Repositories;

public class ParcelDelayPredictionRepository : IParcelDelayPredictionRepository
{
    public const string ProviderCode = "MLNET";
    public const string ModelCode = "PARCEL_DELAY_SDCALOGISTICREGRESSION_V1";
    public const string PromptKey = "PARCEL_DELAY_PREDICTION";

    private readonly ApplicationDbContext _context;

    public ParcelDelayPredictionRepository(ApplicationDbContext context) =>
        _context = context;

    public Task<List<Parcel>> GetCompletedParcelsForTrainingAsync() =>
        _context.Parcels
            .AsNoTracking()
            .Where(parcel =>
                parcel.IsActive &&
                !parcel.IsDeleted &&
                parcel.DeliveredAt.HasValue &&
                parcel.ExpectedDeliveryDate.HasValue &&
                parcel.RouteId.HasValue)
            .Include(parcel => parcel.Route)
                .ThenInclude(route => route!.OriginHub)
            .Include(parcel => parcel.Route)
                .ThenInclude(route => route!.DestinationHub)
            .ToListAsync();

    public Task<List<Parcel>> GetEligibleParcelsForPredictionAsync() =>
        _context.Parcels
            .AsNoTracking()
            .Where(parcel =>
                parcel.IsActive &&
                !parcel.IsDeleted &&
                !parcel.DeliveredAt.HasValue &&
                parcel.ExpectedDeliveryDate.HasValue &&
                parcel.Route != null &&
                parcel.Route.IsActive &&
                !parcel.Route.IsDeleted &&
                !parcel.CurrentStatus.IsTerminal &&
                parcel.Route.OriginHub.IsActive &&
                !parcel.Route.OriginHub.IsDeleted &&
                parcel.Route.DestinationHub.IsActive &&
                !parcel.Route.DestinationHub.IsDeleted)
            .Include(parcel => parcel.CurrentStatus)
            .Include(parcel => parcel.Route)
                .ThenInclude(route => route!.OriginHub)
            .Include(parcel => parcel.Route)
                .ThenInclude(route => route!.DestinationHub)
            .OrderBy(parcel => parcel.ExpectedDeliveryDate)
            .ThenBy(parcel => parcel.ParcelId)
            .ToListAsync();

    public async Task<(AiModel Model, AiPrompt Prompt)> GetOrCreateModelReferencesAsync(
        int requestedBy)
    {
        var provider = await _context.AiProviders
            .FirstOrDefaultAsync(item => item.ProviderCode == ProviderCode);
        if (provider is null)
        {
            provider = new AiProvider
            {
                ProviderName = "ML.NET",
                ProviderCode = ProviderCode,
                Description = "Local machine-learning models hosted by this application.",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = requestedBy
            };
            _context.AiProviders.Add(provider);
            await _context.SaveChangesAsync();
        }
        else
        {
            provider.IsActive = true;
            provider.IsDeleted = false;
        }

        var model = await _context.AiModels.FirstOrDefaultAsync(item =>
            item.AiProviderId == provider.AiProviderId &&
            item.ModelCode == ModelCode);
        if (model is null)
        {
            model = new AiModel
            {
                AiProviderId = provider.AiProviderId,
                ModelCode = ModelCode,
                DisplayName = "Parcel Delivery Delay Classifier",
                Description = "ML.NET SDCA logistic regression model.",
                IsActive = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = requestedBy,
                AiProvider = provider
            };
            _context.AiModels.Add(model);
        }
        else
        {
            model.IsDeleted = false;
        }

        var prompt = await _context.AiPrompts
            .FirstOrDefaultAsync(item => item.PromptKey == PromptKey);
        if (prompt is null)
        {
            prompt = new AiPrompt
            {
                PromptKey = PromptKey,
                PromptName = "Parcel Delivery Delay Prediction",
                Description = "Tracks local ML.NET delivery-delay model training and prediction requests.",
                ModuleName = "Parcel Operations",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = requestedBy
            };
            _context.AiPrompts.Add(prompt);
        }
        else
        {
            prompt.IsActive = true;
            prompt.IsDeleted = false;
        }

        await _context.SaveChangesAsync();
        return (model, prompt);
    }

    public Task<AiModel?> GetModelAsync() =>
        _context.AiModels
            .AsNoTracking()
            .Include(model => model.AiProvider)
            .FirstOrDefaultAsync(model =>
                model.ModelCode == ModelCode &&
                model.AiProvider.ProviderCode == ProviderCode &&
                model.IsActive &&
                !model.IsDeleted &&
                model.AiProvider.IsActive &&
                !model.AiProvider.IsDeleted);

    public Task<AiPrompt?> GetPromptAsync() =>
        _context.AiPrompts
            .AsNoTracking()
            .FirstOrDefaultAsync(prompt =>
                prompt.PromptKey == PromptKey &&
                prompt.IsActive &&
                !prompt.IsDeleted);

    public async Task RecordSuccessfulTrainingAsync(
        int modelId,
        string description,
        int updatedBy,
        AiRequest request)
    {
        var model = await _context.AiModels
            .FirstAsync(item => item.AiModelId == modelId);
        model.IsActive = true;
        model.IsDeleted = false;
        model.Description = description;
        model.UpdatedAt = DateTime.UtcNow;
        model.UpdatedBy = updatedBy;
        _context.AiRequests.Add(request);
        await _context.SaveChangesAsync();
    }

    public async Task AddRequestsAsync(IReadOnlyCollection<AiRequest> requests)
    {
        if (requests.Count == 0)
            return;

        _context.AiRequests.AddRange(requests);
        await _context.SaveChangesAsync();
    }

    public Task<int> GetCompletedPredictionCountAsync() =>
        _context.AiRequests.CountAsync(request =>
            request.RequestPurpose == "PARCEL_DELAY_PREDICTION" &&
            request.RequestStatus == "Succeeded");

    public Task<AiRequest?> GetLatestSuccessfulTrainingRequestAsync() =>
        _context.AiRequests
            .AsNoTracking()
            .Where(request =>
                request.RequestPurpose == "PARCEL_DELAY_MODEL_TRAINING" &&
                request.RequestStatus == "Succeeded")
            .OrderByDescending(request => request.RequestedAt)
            .FirstOrDefaultAsync();

    public Task<AiRequest?> GetLatestTrainingAttemptAsync() =>
        _context.AiRequests
            .AsNoTracking()
            .Where(request =>
                request.RequestPurpose == "PARCEL_DELAY_MODEL_TRAINING")
            .OrderByDescending(request => request.RequestedAt)
            .FirstOrDefaultAsync();
}
