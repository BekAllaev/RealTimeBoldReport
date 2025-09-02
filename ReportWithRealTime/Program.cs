using BoldReports.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.WebSockets;
using ReportWithRealTime.Client.Pages;
using ReportWithRealTime.Components;
using ReportWithRealTime.Services;

Bold.Licensing.BoldLicenseProvider.RegisterLicense("gFVmCnZi2bVTJyccaSRxm5thTNY+P9ONI5ME6zQR0p0=");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri("https://localhost:7276"); 
});

builder.Services.AddMemoryCache();
builder.Services.AddControllers();

builder.Services.AddSingleton<IJsonDataSourceUpdater, JsonDataSourceUpdater>();

builder.Services.AddScoped(sp =>
{
    // /hubs/statistics — путь, который ты замапил на сервере
    return new HubConnectionBuilder()
        .WithUrl("https://localhost:7276/hubs/statistics")
        .WithAutomaticReconnect()
        .Build();
});

var app = builder.Build();

ReportConfig.DefaultSettings = new ReportSettings().RegisterExtensions(new List<string> {"BoldReports.Data.WebData",
                                                                                        "BoldReports.Data.PostgreSQL",
                                                                                        "BoldReports.Data.Excel",
                                                                                        "BoldReports.Data.Csv",
                                                                                        "BoldReports.Data.Oracle",
                                                                                        "BoldReports.Data.ElasticSearch",
                                                                                        "BoldReports.Data.Snowflake",
                                                                                        "BoldReports.Data.SSAS"});


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(ReportWithRealTime.Client._Imports).Assembly);
app.MapControllers();

app.Run();
