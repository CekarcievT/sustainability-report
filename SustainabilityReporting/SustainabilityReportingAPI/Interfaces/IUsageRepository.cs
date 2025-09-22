using DataContext.Shared.Models;

namespace SustainabilityReportingAPI.Interfaces
{
    public interface IUsageRepository
    {
        public Task<List<MonthlyUsage>> GetMonthlyUsage();
    }
}
