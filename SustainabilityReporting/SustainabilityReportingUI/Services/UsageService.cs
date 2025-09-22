using DTOs.Shared;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;

namespace SustainabilityReportingUI.Services
{
    public class UsageService
    {
        private readonly HttpClient _httpClient;

        public UsageService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<MonthlyUsageDto>?> GetMonthlyUsageAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<MonthlyUsageDto>>($"api/usage");
        }

        // Uploads a CSV file to the API
        public async Task<HttpResponseMessage> UploadCsvAsync(IBrowserFile file)
        {
            var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024)); // 10 MB limit
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "file", file.Name);

            // Adjust the endpoint as needed
            return await _httpClient.PostAsync("api/usage/upload", content);
        }
    }
}
