using System.Collections.Generic;

namespace FlowVacSitePlanner.Domain.Planning;

public sealed class PlanningResult
{
    public bool IsSuccess { get; set; }

    public string ErrorCode { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;

    public string GrammarVersionId { get; set; } = string.Empty;

    public string? ScenarioId { get; set; }

    public List<PlanningResultOption> Options { get; set; } = new();

    public static PlanningResult Failure(string errorCode, string errorMessage)
    {
        return new PlanningResult
        {
            IsSuccess = false,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage
        };
    }
}
