using DataContext.Shared.Data;
using DataContext.Shared.Models;
using Microsoft.EntityFrameworkCore;
using SustainabilityReportingAPI.Interfaces;

namespace SustainabilityReportingAPI.Repository
{
    public class UsageRepository : IUsageRepository
    {
        ApplicationDbContext context;
        public UsageRepository(ApplicationDbContext applicationDbContext)
        { 
            context = applicationDbContext;
        }

        public async Task<List<MonthlyUsage>> GetMonthlyUsage()
        {
            return await context.MonthlyUsages.ToListAsync();
        }
    }
}
