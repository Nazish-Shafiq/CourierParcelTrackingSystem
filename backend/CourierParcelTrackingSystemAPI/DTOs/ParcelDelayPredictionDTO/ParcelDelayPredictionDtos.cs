namespace CourierParcelTrackingSystem.API.DTOs.ParcelDelayPredictionDTO;

public class ParcelDelayPredictionDto
{
    public long ParcelId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string RouteName { get; set; } = string.Empty;
    public string OriginHubName { get; set; } = string.Empty;
    public string DestinationHubName { get; set; } = string.Empty;
    public DateTime ExpectedDeliveryDate { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public float DelayProbability { get; set; }
}

public class ParcelDelayPredictionsResponseDto
{
    public bool IsModelAvailable { get; set; }
    public bool TrainingReady { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Algorithm { get; set; } = string.Empty;
    public List<ParcelDelayPredictionDto> Predictions { get; set; } = new();
}

public class ParcelDelayModelStatusDto
{
    public bool IsModelAvailable { get; set; }
    public bool TrainingReady { get; set; }
    public string ReadinessMessage { get; set; } = string.Empty;
    public bool EvaluationAvailable { get; set; }
    public string EvaluationMessage { get; set; } = string.Empty;
    public string Algorithm { get; set; } = string.Empty;
    public int ValidTrainingSamples { get; set; }
    public int OnTimeSamples { get; set; }
    public int DelayedSamples { get; set; }
    public int TotalCompletedDeliveries { get; set; }
    public int OnTimeDeliveries { get; set; }
    public int DelayedDeliveries { get; set; }
    public double? OnTimeDeliveryPercentage { get; set; }
    public double? DelayedDeliveryPercentage { get; set; }
    public double? AverageDeliveryDurationHours { get; set; }
    public int CompletedPredictionCount { get; set; }
    public DateTime? LastTrainedAt { get; set; }
    public string? LastTrainingStatus { get; set; }
    public string? LastTrainingMessage { get; set; }
    public float? Accuracy { get; set; }
    public float? AreaUnderRocCurve { get; set; }
    public float? F1Score { get; set; }
    public float? Precision { get; set; }
    public float? Recall { get; set; }
}
