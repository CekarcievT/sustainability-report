using DTOs.Shared;
using SustainabilityReportingAPI.Interfaces;

namespace SustainabilityReportingAPI.Services
{
    public class UsageService : IUsageService
    {
        private readonly IUsageRepository _usageRepository;

        public UsageService(IUsageRepository usageRepository)
        {
            _usageRepository = usageRepository;
        }

        public async Task<List<MonthlyUsageDto>> GetMonthlyUsageAsync()
        {
            var monthlyUsageList = await _usageRepository.GetMonthlyUsage();
            var result = monthlyUsageList
                .Select(x => new MonthlyUsageDto
                {
                    VendorId = x.VendorId,
                    PeriodStart = x.PeriodStart,
                    UsageAmount = x.UsageAmount
                })
                .ToList();

            return result;
        }
    }
}
