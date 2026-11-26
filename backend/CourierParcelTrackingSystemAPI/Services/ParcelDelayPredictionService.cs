using System.Text.Json;
using CourierParcelTrackingSystem.API.DTOs.ParcelDelayPredictionDTO;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using CourierParcelTrackingSystem.API.Services.Interfaces;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace CourierParcelTrackingSystem.API.Services;

public class ParcelDelayPredictionService : IParcelDelayPredictionService
{
    private const double MinimumAuc = 0.6;
    private const string AlgorithmName = "ML.NET SDCA logistic regression";
    private static readonly SemaphoreSlim TrainingLock = new(1, 1);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IParcelDelayPredictionRepository _repository;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ParcelDelayPredictionService> _logger;
    private readonly string _modelPath;

    public ParcelDelayPredictionService(
        IParcelDelayPredictionRepository repository,
        IWebHostEnvironment environment,
        ILogger<ParcelDelayPredictionService> logger)
    {
        _repository = repository;
        _environment = environment;
        _logger = logger;
        _modelPath = Path.Combine(
            environment.ContentRootPath,
            "App_Data",
            "MLModels",
            "parcel-delay-classifier.zip");
    }

    public async Task<ParcelDelayModelStatusDto> GetAdminStatusAsync()
    {
        var trainingData = BuildTrainingData(
            await _repository.GetCompletedParcelsForTrainingAsync());
        var readiness = GetReadiness(trainingData);
        var model = await _repository.GetModelAsync();
        var latestTraining = await _repository.GetLatestSuccessfulTrainingRequestAsync();
        var latestTrainingAttempt = await _repository.GetLatestTrainingAttemptAsync();
        var evaluation = DeserializeEvaluation(latestTraining);
        var evaluationAvailable = IsEvaluationAvailable(evaluation);
        var isModelAvailable = readiness.IsReady &&
            model is not null &&
            File.Exists(_modelPath);
        var readinessMessage = isModelAvailable &&
            latestTrainingAttempt?.RequestStatus == "Succeeded"
                ? "Model trained successfully using actual delivery history."
                : readiness.Message;
        var evaluationMessage = evaluation?.EvaluationMessage
            ?? (evaluationAvailable
                ? "Held-out evaluation metrics from the last successful training run are available."
                : readiness.IsReady
                    ? "Held-out evaluation is not available until a model training run completes."
                    : "Held-out evaluation is unavailable until both delivery outcome classes are present.");

        return new ParcelDelayModelStatusDto
        {
            IsModelAvailable = isModelAvailable,
            TrainingReady = readiness.IsReady,
            ReadinessMessage = readinessMessage,
            EvaluationAvailable = evaluationAvailable,
            EvaluationMessage = evaluationMessage,
            Algorithm = AlgorithmName,
            ValidTrainingSamples = trainingData.Rows.Count,
            OnTimeSamples = trainingData.OnTimeCount,
            DelayedSamples = trainingData.DelayedCount,
            TotalCompletedDeliveries = trainingData.Rows.Count,
            OnTimeDeliveries = trainingData.OnTimeCount,
            DelayedDeliveries = trainingData.DelayedCount,
            OnTimeDeliveryPercentage = GetPercentage(
                trainingData.OnTimeCount,
                trainingData.Rows.Count),
            DelayedDeliveryPercentage = GetPercentage(
                trainingData.DelayedCount,
                trainingData.Rows.Count),
            AverageDeliveryDurationHours =
                trainingData.AverageDeliveryDurationHours,
            CompletedPredictionCount =
                await _repository.GetCompletedPredictionCountAsync(),
            LastTrainedAt = latestTraining?.CompletedAt,
            LastTrainingStatus = latestTrainingAttempt?.RequestStatus,
            LastTrainingMessage = latestTrainingAttempt?.ErrorMessage,
            Accuracy = evaluation?.Accuracy,
            AreaUnderRocCurve = evaluation?.AreaUnderRocCurve,
            F1Score = evaluation?.F1Score,
            Precision = evaluation?.Precision,
            Recall = evaluation?.Recall
        };
    }

    public async Task<ParcelDelayModelStatusDto> TrainAsync(int requestedBy)
    {
        await TrainingLock.WaitAsync();
        try
        {
            var trainingData = BuildTrainingData(
                await _repository.GetCompletedParcelsForTrainingAsync());
            var readiness = GetReadiness(trainingData);
            var (modelReference, prompt) =
                await _repository.GetOrCreateModelReferencesAsync(requestedBy);

            if (!readiness.IsReady)
            {
                var blockedAttempt = CreateRequest(
                    modelReference,
                    prompt,
                    requestedBy,
                    "PARCEL_DELAY_MODEL_TRAINING",
                    "Failed",
                    JsonSerializer.Serialize(new
                    {
                        trainingData.Rows.Count,
                        trainingData.OnTimeCount,
                        trainingData.DelayedCount
                    }, JsonOptions),
                    JsonSerializer.Serialize(new
                    {
                        trainingReady = false,
                        readinessMessage = readiness.Message
                    }, JsonOptions),
                    readiness.Message);
                await _repository.AddRequestsAsync(new[] { blockedAttempt });
                return await GetAdminStatusAsync();
            }

            var evaluationAvailable = TrySplitTrainingData(
                trainingData.Rows,
                out var trainRows,
                out var testRows);
            var rowsForTraining = evaluationAvailable
                ? trainRows
                : trainingData.Rows;
            var mlContext = new MLContext(seed: 1247);
            var trainingView = mlContext.Data.LoadFromEnumerable(rowsForTraining);
            var pipeline = BuildPipeline(mlContext);
            var trainedModel = pipeline.Fit(trainingView);

            TrainingEvaluationSnapshot evaluation;
            if (evaluationAvailable)
            {
                var testView = mlContext.Data.LoadFromEnumerable(testRows);
                var metrics = mlContext.BinaryClassification.Evaluate(
                    trainedModel.Transform(testView),
                    labelColumnName: nameof(DelayTrainingRow.Label));
                evaluation = ToEvaluationSnapshot(metrics, testRows.Count);

                if (!double.IsFinite(metrics.AreaUnderRocCurve) ||
                    metrics.AreaUnderRocCurve < MinimumAuc)
                {
                    var evaluationFailure = !double.IsFinite(metrics.AreaUnderRocCurve)
                        ? "The held-out evaluation AUC could not be calculated; the model was not enabled."
                        : $"The held-out evaluation AUC ({metrics.AreaUnderRocCurve:F3}) did not meet the minimum {MinimumAuc:F2}; the model was not enabled.";
                    await _repository.AddRequestsAsync(new[]
                    {
                        CreateRequest(
                            modelReference,
                            prompt,
                            requestedBy,
                            "PARCEL_DELAY_MODEL_TRAINING",
                            "Failed",
                            JsonSerializer.Serialize(new
                            {
                                trainingData.Rows.Count,
                                trainingData.OnTimeCount,
                                trainingData.DelayedCount,
                                trainingSamplesUsed = rowsForTraining.Count,
                                evaluationSamples = testRows.Count
                            }, JsonOptions),
                            JsonSerializer.Serialize(evaluation, JsonOptions),
                            evaluationFailure)
                    });

                    return await GetAdminStatusAsync();
                }
            }
            else
            {
                evaluation = new TrainingEvaluationSnapshot
                {
                    EvaluationAvailable = false,
                    EvaluationMessage =
                        "Held-out evaluation is unavailable because the available records cannot be split while keeping both outcome classes in the training and evaluation sets. The model was trained using all valid records; its predictions have not been independently evaluated."
                };
            }

            var modelDirectory = Path.GetDirectoryName(_modelPath)
                ?? throw new InvalidOperationException("The ML model directory is unavailable.");
            Directory.CreateDirectory(modelDirectory);
            var temporaryModelPath = $"{_modelPath}.{Guid.NewGuid():N}.tmp";
            try
            {
                mlContext.Model.Save(trainedModel, trainingView.Schema, temporaryModelPath);
                File.Move(temporaryModelPath, _modelPath, overwrite: true);
            }
            finally
            {
                if (File.Exists(temporaryModelPath))
                    File.Delete(temporaryModelPath);
            }

            var successfulTrainingRequest = CreateRequest(
                modelReference,
                prompt,
                requestedBy,
                "PARCEL_DELAY_MODEL_TRAINING",
                "Succeeded",
                JsonSerializer.Serialize(new
                {
                    trainingData.Rows.Count,
                    trainingData.OnTimeCount,
                    trainingData.DelayedCount,
                    trainingSamplesUsed = rowsForTraining.Count,
                    evaluationSamples = testRows.Count,
                    evaluationAvailable
                }, JsonOptions),
                JsonSerializer.Serialize(evaluation, JsonOptions),
                null);
            var trainingDescription = evaluationAvailable
                ? $"{AlgorithmName}. Held-out AUC: {evaluation.AreaUnderRocCurve:F3}; trained on {rowsForTraining.Count} actual validated deliveries."
                : $"{AlgorithmName}. Held-out evaluation unavailable; trained on {rowsForTraining.Count} actual validated deliveries.";
            await _repository.RecordSuccessfulTrainingAsync(
                modelReference.AiModelId,
                trainingDescription,
                requestedBy,
                successfulTrainingRequest);

            _logger.LogInformation(
                "Parcel delay model trained with {SampleCount} actual validated deliveries. Evaluation available: {EvaluationAvailable}.",
                rowsForTraining.Count,
                evaluationAvailable);

            return await GetAdminStatusAsync();
        }
        finally
        {
            TrainingLock.Release();
        }
    }

    public async Task<ParcelDelayPredictionsResponseDto> GetDispatcherPredictionsAsync(
        int requestedBy)
    {
        var trainingData = BuildTrainingData(
            await _repository.GetCompletedParcelsForTrainingAsync());
        var readiness = GetReadiness(trainingData);
        if (!readiness.IsReady)
        {
            return new ParcelDelayPredictionsResponseDto
            {
                IsModelAvailable = false,
                TrainingReady = false,
                Message = $"Predictions are unavailable because the valid delivery history does not contain both on-time and delayed outcomes. {readiness.Message}",
                Algorithm = AlgorithmName
            };
        }

        var eligibleParcels =
            await _repository.GetEligibleParcelsForPredictionAsync();
        var model = await _repository.GetModelAsync();

        if (model is null || !File.Exists(_modelPath))
        {
            return new ParcelDelayPredictionsResponseDto
            {
                IsModelAvailable = false,
                TrainingReady = true,
                Message = "No trained delay model is active. An administrator must train a model before predictions can be shown.",
                Algorithm = AlgorithmName
            };
        }

        var evaluation = DeserializeEvaluation(
            await _repository.GetLatestSuccessfulTrainingRequestAsync());
        var evaluationAvailable = IsEvaluationAvailable(evaluation);
        var prompt = await _repository.GetPromptAsync();
        if (prompt is null)
        {
            return new ParcelDelayPredictionsResponseDto
            {
                IsModelAvailable = false,
                TrainingReady = true,
                Message = "The trained model is missing its active AI prompt record. No prediction request was saved.",
                Algorithm = AlgorithmName
            };
        }

        await TrainingLock.WaitAsync();
        try
        {
            var mlContext = new MLContext();
            var loadedModel = mlContext.Model.Load(_modelPath, out _);
            var predictionEngine =
                mlContext.Model.CreatePredictionEngine<DelayPredictionInput, DelayPredictionOutput>(loadedModel);
            var predictions = new List<ParcelDelayPredictionDto>();
            var requests = new List<AiRequest>(eligibleParcels.Count);

            foreach (var parcel in eligibleParcels)
            {
                if (!TryCreatePredictionInput(parcel, out var input))
                    continue;

                var result = predictionEngine.Predict(input);
                if (!float.IsFinite(result.Probability))
                    throw new InvalidDataException(
                        "The trained model returned a non-finite delay probability.");

                var riskLevel = GetRiskLevel(result.Probability);
                var route = parcel.Route!;
                var prediction = new ParcelDelayPredictionDto
                {
                    ParcelId = parcel.ParcelId,
                    TrackingNumber = parcel.TrackingNumber,
                    RouteName = route.RouteName,
                    OriginHubName = route.OriginHub.HubName,
                    DestinationHubName = route.DestinationHub.HubName,
                    ExpectedDeliveryDate = parcel.ExpectedDeliveryDate!.Value,
                    RiskLevel = riskLevel,
                    DelayProbability = result.Probability
                };
                predictions.Add(prediction);
                requests.Add(CreateRequest(
                    model,
                    prompt,
                    requestedBy,
                    "PARCEL_DELAY_PREDICTION",
                    "Succeeded",
                    JsonSerializer.Serialize(ToFeatureSnapshot(input), JsonOptions),
                    JsonSerializer.Serialize(prediction, JsonOptions),
                    null,
                    parcel.ParcelId));
            }

            await _repository.AddRequestsAsync(requests);
            return new ParcelDelayPredictionsResponseDto
            {
                IsModelAvailable = true,
                TrainingReady = true,
                Message = !evaluationAvailable
                        ? predictions.Count == 0
                            ? "There are no eligible parcels with complete route and schedule data. The trained model has no independent held-out evaluation; treat its risk scores as exploratory."
                            : "Predictions use a trained model without an independent held-out evaluation. Treat the risk scores as exploratory."
                        : predictions.Count == 0
                            ? "There are no eligible parcels with complete route and schedule data to predict."
                            : $"Delay risk predictions are based on the evaluated {AlgorithmName} model.",
                Algorithm = AlgorithmName,
                Predictions = predictions
            };
        }
        finally
        {
            TrainingLock.Release();
        }
    }

    private static IEstimator<ITransformer> BuildPipeline(MLContext mlContext) =>
        mlContext.Transforms.Categorical.OneHotEncoding(
                "OriginHubFeatures",
                nameof(DelayTrainingRow.OriginHubCode))
            .Append(mlContext.Transforms.Categorical.OneHotEncoding(
                "DestinationHubFeatures",
                nameof(DelayTrainingRow.DestinationHubCode)))
            .Append(mlContext.Transforms.Categorical.OneHotEncoding(
                "RouteFeatures",
                nameof(DelayTrainingRow.RouteCode)))
            .Append(mlContext.Transforms.Concatenate(
                "Features",
                "OriginHubFeatures",
                "DestinationHubFeatures",
                "RouteFeatures",
                nameof(DelayTrainingRow.TotalWeightKg),
                nameof(DelayTrainingRow.ScheduledLeadHours),
                nameof(DelayTrainingRow.BookingMonth),
                nameof(DelayTrainingRow.BookingDayOfWeek)))
            .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
                labelColumnName: nameof(DelayTrainingRow.Label),
                featureColumnName: "Features"));

    private static TrainingData BuildTrainingData(IEnumerable<Parcel> parcels)
    {
        var rows = new List<DelayTrainingRow>();
        var onTimeCount = 0;
        var delayedCount = 0;
        var totalDeliveryDurationHours = 0d;
        var evaluatedAt = DateTime.UtcNow;

        foreach (var parcel in parcels
            .Where(parcel => parcel.IsActive && !parcel.IsDeleted)
            .DistinctBy(parcel => parcel.ParcelId))
        {
            if (!TryCreatePredictionInput(parcel, out var features) ||
                !parcel.DeliveredAt.HasValue)
                continue;

            var deliveredAt = parcel.DeliveredAt.Value;
            if (deliveredAt > evaluatedAt)
                continue;

            var actualDeliveryDurationHours =
                (deliveredAt - parcel.BookingDate).TotalHours;
            if (!double.IsFinite(actualDeliveryDurationHours) ||
                actualDeliveryDurationHours < 0)
                continue;

            totalDeliveryDurationHours += actualDeliveryDurationHours;
            var isDelayed = deliveredAt > parcel.ExpectedDeliveryDate!.Value;
            if (isDelayed)
                delayedCount++;
            else
                onTimeCount++;

            rows.Add(new DelayTrainingRow
            {
                Label = isDelayed,
                OriginHubCode = features.OriginHubCode,
                DestinationHubCode = features.DestinationHubCode,
                RouteCode = features.RouteCode,
                TotalWeightKg = features.TotalWeightKg,
                ScheduledLeadHours = features.ScheduledLeadHours,
                BookingMonth = features.BookingMonth,
                BookingDayOfWeek = features.BookingDayOfWeek
            });
        }

        double? averageDeliveryDurationHours = rows.Count == 0
            ? null
            : totalDeliveryDurationHours / rows.Count;
        return new TrainingData(
            rows,
            onTimeCount,
            delayedCount,
            averageDeliveryDurationHours);
    }

    private static bool TryCreatePredictionInput(
        Parcel parcel,
        out DelayPredictionInput input)
    {
        input = new DelayPredictionInput();
        var route = parcel.Route;
        var expectedDeliveryDate = parcel.ExpectedDeliveryDate;
        if (route is null ||
            expectedDeliveryDate is null ||
            parcel.TotalWeightKg <= 0 ||
            expectedDeliveryDate.Value < parcel.BookingDate ||
            string.IsNullOrWhiteSpace(route.RouteCode) ||
            string.IsNullOrWhiteSpace(route.OriginHub?.HubCode) ||
            string.IsNullOrWhiteSpace(route.DestinationHub?.HubCode))
            return false;

        var leadHours =
            (expectedDeliveryDate.Value - parcel.BookingDate).TotalHours;
        if (!double.IsFinite(leadHours) || leadHours < 0)
            return false;

        input = new DelayPredictionInput
        {
            OriginHubCode = route.OriginHub.HubCode,
            DestinationHubCode = route.DestinationHub.HubCode,
            RouteCode = route.RouteCode,
            TotalWeightKg = (float)parcel.TotalWeightKg,
            ScheduledLeadHours = (float)leadHours,
            BookingMonth = parcel.BookingDate.Month,
            BookingDayOfWeek = (float)parcel.BookingDate.DayOfWeek,
            Label = false
        };
        return true;
    }

    private static bool TrySplitTrainingData(
        List<DelayTrainingRow> rows,
        out List<DelayTrainingRow> train,
        out List<DelayTrainingRow> test)
    {
        var random = new Random(1247);
        train = new List<DelayTrainingRow>();
        test = new List<DelayTrainingRow>();
        var outcomeClasses = rows.GroupBy(row => row.Label).ToList();
        if (outcomeClasses.Count != 2 || outcomeClasses.Any(group => group.Count() < 2))
            return false;

        foreach (var classRows in outcomeClasses)
        {
            var shuffledRows = classRows.OrderBy(_ => random.Next()).ToList();
            var testCount = Math.Clamp(shuffledRows.Count / 5, 1, shuffledRows.Count - 1);
            test.AddRange(shuffledRows.Take(testCount));
            train.AddRange(shuffledRows.Skip(testCount));
        }

        return train.Select(row => row.Label).Distinct().Count() == 2 &&
            test.Select(row => row.Label).Distinct().Count() == 2;
    }

    private static (bool IsReady, string Message) GetReadiness(TrainingData data)
    {
        if (data.Rows.Count == 0)
        {
            return (false,
                "No valid completed deliveries are available. Training requires actual delivery records with complete schedule and route features.");
        }

        if (data.OnTimeCount == 0 && data.DelayedCount > 0)
        {
            return (false,
                "Training is not possible yet because the available delivery history contains only delayed deliveries. Actual on-time delivery examples are required for binary classification.");
        }

        if (data.DelayedCount == 0 && data.OnTimeCount > 0)
        {
            return (false,
                "Training is not possible yet because the available delivery history contains only on-time deliveries. Actual delayed delivery examples are required for binary classification.");
        }

        return (true,
            "Valid on-time and delayed delivery examples are available; training can proceed using actual delivery history.");
    }

    private static double? GetPercentage(int count, int total) =>
        total == 0 ? null : count * 100d / total;

    private static TrainingEvaluationSnapshot ToEvaluationSnapshot(
        CalibratedBinaryClassificationMetrics metrics,
        int evaluationSampleCount) =>
        new()
        {
            EvaluationAvailable = true,
            EvaluationMessage =
                $"Held-out metrics were calculated using {evaluationSampleCount} actual delivery records; interpret small-sample results cautiously.",
            Accuracy = ToFiniteMetric(metrics.Accuracy),
            AreaUnderRocCurve = ToFiniteMetric(metrics.AreaUnderRocCurve),
            F1Score = ToFiniteMetric(metrics.F1Score),
            Precision = ToFiniteMetric(metrics.PositivePrecision),
            Recall = ToFiniteMetric(metrics.PositiveRecall)
        };

    private static float? ToFiniteMetric(double value) =>
        double.IsFinite(value) ? (float)value : null;

    private static TrainingEvaluationSnapshot? DeserializeEvaluation(
        AiRequest? trainingRequest) =>
        trainingRequest?.OutputSnapshot is { Length: > 0 } snapshot
            ? JsonSerializer.Deserialize<TrainingEvaluationSnapshot>(
                snapshot,
                JsonOptions)
            : null;

    private static bool IsEvaluationAvailable(
        TrainingEvaluationSnapshot? evaluation) =>
        evaluation?.EvaluationAvailable ??
        (evaluation?.AreaUnderRocCurve.HasValue == true);

    private static string GetRiskLevel(float delayProbability) =>
        delayProbability >= 0.7f
            ? "High"
            : delayProbability >= 0.4f
                ? "Medium"
                : "Low";

    private static object ToFeatureSnapshot(DelayPredictionInput input) => new
    {
        input.OriginHubCode,
        input.DestinationHubCode,
        input.RouteCode,
        input.TotalWeightKg,
        input.ScheduledLeadHours,
        input.BookingMonth,
        input.BookingDayOfWeek
    };

    private static AiRequest CreateRequest(
        AiModel model,
        AiPrompt prompt,
        int requestedBy,
        string purpose,
        string status,
        string inputSnapshot,
        string outputSnapshot,
        string? errorMessage,
        long? parcelId = null)
    {
        var requestedAt = DateTime.UtcNow;
        return new AiRequest
        {
            AiPromptId = prompt.AiPromptId,
            AiModelId = model.AiModelId,
            RequestedBy = requestedBy,
            ParcelId = parcelId,
            RequestPurpose = purpose,
            InputSnapshot = inputSnapshot,
            OutputSnapshot = outputSnapshot,
            RequestStatus = status,
            ErrorMessage = errorMessage,
            RequestedAt = requestedAt,
            CompletedAt = requestedAt,
            AiPrompt = prompt,
            AiModel = model
        };
    }

    private sealed record TrainingData(
        List<DelayTrainingRow> Rows,
        int OnTimeCount,
        int DelayedCount,
        double? AverageDeliveryDurationHours);

    private sealed class TrainingEvaluationSnapshot
    {
        public bool? EvaluationAvailable { get; set; }
        public string? EvaluationMessage { get; set; }
        public float? Accuracy { get; set; }
        public float? AreaUnderRocCurve { get; set; }
        public float? F1Score { get; set; }
        public float? Precision { get; set; }
        public float? Recall { get; set; }
    }

    private sealed class DelayTrainingRow
    {
        public bool Label { get; set; }
        public string OriginHubCode { get; set; } = string.Empty;
        public string DestinationHubCode { get; set; } = string.Empty;
        public string RouteCode { get; set; } = string.Empty;
        public float TotalWeightKg { get; set; }
        public float ScheduledLeadHours { get; set; }
        public float BookingMonth { get; set; }
        public float BookingDayOfWeek { get; set; }
    }

    private class DelayPredictionInput
    {
        public string OriginHubCode { get; set; } = string.Empty;
        public string DestinationHubCode { get; set; } = string.Empty;
        public string RouteCode { get; set; } = string.Empty;
        public float TotalWeightKg { get; set; }
        public float ScheduledLeadHours { get; set; }
        public float BookingMonth { get; set; }
        public float BookingDayOfWeek { get; set; }
        // Required for ML.NET scoring schema; this constant is excluded from the feature vector.
        public bool Label { get; set; }
    }

    private sealed class DelayPredictionOutput
    {
        public bool PredictedLabel { get; set; }
        public float Probability { get; set; }
        public float Score { get; set; }
    }
}
