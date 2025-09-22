using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using SustainabilityReportingUI.Services;

namespace SustainabilityReportingUI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddSingleton<MockAuthService>();
            builder.Services.AddTransient<UserHeaderHandler>();

            builder.Services.AddHttpClient<UsageService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7155/");
            })
            .AddHttpMessageHandler<UserHeaderHandler>();


            builder.Services.AddMudServices();

            await builder.Build().RunAsync();
        }
    }
}
