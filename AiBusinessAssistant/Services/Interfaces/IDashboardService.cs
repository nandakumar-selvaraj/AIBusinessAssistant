using AiBusinessAssistant.DTOs.Dashboard;

namespace AiBusinessAssistant.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken);
}
