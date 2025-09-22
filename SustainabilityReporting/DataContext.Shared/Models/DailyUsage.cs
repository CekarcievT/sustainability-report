using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataContext.Shared.Models
{
    public class DailyUsage
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        public string VendorId { get; set; }
        public DateTime PeriodStart { get; set; }
        public double UsageAmount { get; set; }
    }
}
