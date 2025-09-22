using DTOs.Shared;

namespace SustainabilityReportingAPI.Interfaces
{
    public interface IUsageService
    {
        public Task<List<MonthlyUsageDto>> GetMonthlyUsageAsync();
    }
}
