namespace DTOs.Shared
{
    public class MonthlyUsageDto
    {
        public string VendorId { get; set; }
        public DateTime PeriodStart { get; set; }
        public double UsageAmount { get; set; }
    }
}