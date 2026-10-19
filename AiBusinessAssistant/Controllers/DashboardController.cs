using AiBusinessAssistant.DTOs.Dashboard;
using AiBusinessAssistant.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AiBusinessAssistant.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<DashboardSummaryDto>> GetSummary(CancellationToken cancellationToken)
        {
            var summary = await _dashboardService.GetSummaryAsync(cancellationToken);
            return Ok(summary);
        }
    }
}
