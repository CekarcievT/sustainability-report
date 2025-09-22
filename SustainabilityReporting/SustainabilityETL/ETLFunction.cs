using CsvHelper;
using CsvHelper.Configuration;
using DataContext.Shared.Data;
using DataContext.Shared.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SustainabilityETL;

public class EtlFunction
{
    private readonly ILogger<EtlFunction> _logger;
    private readonly ApplicationDbContext _dbContext;

    public EtlFunction(ILogger<EtlFunction> logger, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public class VenueUsage
    {
        public string venue_id { get; set; }
        public string datetime { get; set; }
        public double kWh { get; set; }
        public double cost { get; set; }
    }

    public class AggregatedCost
    {
        public string VenueId { get; set; }
        public DateTime PeriodStart { get; set; }
        public double TotalCost { get; set; }
    }

    [Function(nameof(EtlFunction))]
    public async Task Run(
        [BlobTrigger("sustainabilitydata/{name}", Connection = "AzureWebJobsStorage")] Stream stream,
        string name)
    {
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ","
        });

        var records = csv.GetRecords<VenueUsage>().ToList();

        // Calculate cost per row
        foreach (var r in records)
            r.cost = r.kWh * r.cost;

        // Group and aggregate
        var daily = records
            .GroupBy(r => new { r.venue_id, Date = Convert.ToDateTime(r.datetime) })
            .Select(g => new AggregatedCost
            {
                VenueId = g.Key.venue_id,
                PeriodStart = g.Key.Date,
                TotalCost = g.Sum(x => x.kWh * x.cost)
            });

        var weekly = records
            .GroupBy(r => new { r.venue_id, Year = Convert.ToDateTime(r.datetime).Year, Week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(Convert.ToDateTime(r.datetime), CalendarWeekRule.FirstDay, DayOfWeek.Monday) })
            .Select(g => new AggregatedCost
            {
                VenueId = g.Key.venue_id,
                PeriodStart = FirstDateOfWeekI(g.Key.Year, g.Key.Week),
                TotalCost = g.Sum(x => x.kWh * x.cost)
            });

        var monthly = records
            .GroupBy(r => new { r.venue_id, Convert.ToDateTime(r.datetime).Year, Convert.ToDateTime(r.datetime).Month })
            .Select(g => new AggregatedCost
            {
                VenueId = g.Key.venue_id,
                PeriodStart = new DateTime(g.Key.Year, g.Key.Month, 1),
                TotalCost = g.Sum(x => x.kWh * x.cost)
            });

        // Combine all results
        var allResults = daily.Concat(weekly).Concat(monthly).ToList();

        // Insert Daily
        var dailyUsages = daily.Select(x => new DailyUsage
        {
            VendorId = x.VenueId,
            PeriodStart = x.PeriodStart,
            UsageAmount = x.TotalCost
        }).ToList();
        _dbContext.DailyUsages.AddRange(dailyUsages);

        // Insert Weekly
        var weeklyUsages = weekly.Select(x => new WeeklyUsage
        {
            VendorId = x.VenueId,
            PeriodStart = x.PeriodStart,
            UsageAmount = x.TotalCost
        }).ToList();
        _dbContext.WeeklyUsages.AddRange(weeklyUsages);

        // Insert Monthly
        var monthlyUsages = monthly.Select(x => new MonthlyUsage
        {
            VendorId = x.VenueId,
            PeriodStart = x.PeriodStart,
            UsageAmount = x.TotalCost
        }).ToList();
        _dbContext.MonthlyUsages.AddRange(monthlyUsages);

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Aggregated costs calculated for {Count} records", allResults.Count);
    }

    // Helper for ISO week start
    private static DateTime FirstDateOfWeekI(int year, int weekOfYear)
    {
        var jan1 = new DateTime(year, 1, 1);
        int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

        var firstThursday = jan1.AddDays(daysOffset);
        var cal = CultureInfo.InvariantCulture.Calendar;
        int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

        var weekNum = weekOfYear;
        if (firstWeek <= 1)
            weekNum -= 1;

        var result = firstThursday.AddDays(weekNum * 7);
        return result.AddDays(-3);
    }
}