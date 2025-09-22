using DTOs.Shared;
using Microsoft.AspNetCore.Mvc;
using SustainabilityReportingAPI.Interfaces;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

namespace SustainabilityReportingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsageController : ControllerBase
    {
        private readonly IUsageService _usageService;
        private readonly IConfiguration _configuration;

        public UsageController(IUsageService usageService, IConfiguration configuration)
        {
            _usageService = usageService;
            _configuration = configuration;
        }

        private (string Role, string? VenueId) GetMockedUserRole()
        {
            // In production, this would come from user claims
            var user = HttpContext.Request.Headers["X-User"].FirstOrDefault();

            return user switch
            {
                "UserA" => ("UserA", "Venue_A"),
                "UserB" => ("UserB", "Venue_B"),
                "Manager" => ("Manager", null),
                _ => ("User", null) // default: no access
            };
        }

        [HttpGet]
        public async Task<ActionResult<List<MonthlyUsageDto>>> Get()
        {
            var (role, venueId) = GetMockedUserRole();
            var allData = await _usageService.GetMonthlyUsageAsync();

            List<MonthlyUsageDto> filtered;
            if (role == "Manager")
            {
                filtered = allData;
            }
            else if (venueId != null)
            {
                filtered = allData.Where(x => x.VendorId == venueId).ToList();
            }
            else
            {
                filtered = new List<MonthlyUsageDto>();
            }

            return Ok(filtered);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadCsv([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // Get connection string and container name from configuration
            var connectionString = _configuration.GetConnectionString("AzureBlobStorage");
            var containerName = _configuration["AzureBlobContainer"];

            // Create blob client and container if not exists
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();

            var fileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
            var blobClient = containerClient.GetBlobClient(fileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            return Ok(new { FileName = fileName, Message = "File uploaded to Azure Blob Storage successfully." });
        }
    }
}
