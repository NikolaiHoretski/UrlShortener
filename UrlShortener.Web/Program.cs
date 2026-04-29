using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using UrlShortener.Web;
using UrlShortener.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBase = builder.Configuration["ApiBaseUrl"];
if (string.IsNullOrWhiteSpace(apiBase))
{
    throw new InvalidOperationException(
        "Укажите ApiBaseUrl в wwwroot/appsettings.json (базовый URL API, со слэшем на конце).");
}

builder.Services.AddScoped(_ => new HttpClient
{
    BaseAddress = new Uri(apiBase.TrimEnd('/') + "/", UriKind.Absolute),
});
builder.Services.AddScoped<UrlApiClient>();

await builder.Build().RunAsync();
