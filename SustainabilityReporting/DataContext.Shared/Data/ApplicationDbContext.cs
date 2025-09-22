using DataContext.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace DataContext.Shared.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            ChangeTracker.LazyLoadingEnabled = false;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {  
        }

        public DbSet<DailyUsage> DailyUsages { get; set; }
        public DbSet<WeeklyUsage> WeeklyUsages { get; set; }
        public DbSet<MonthlyUsage> MonthlyUsages { get; set; }
    }
}
