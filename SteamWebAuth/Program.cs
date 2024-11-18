using SteamWebAuth.Components;
using SteamWebAuthenticator.Interfaces;
using SteamWebAuthenticator.Services;
using Toolbelt.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddHttpClient();
builder.Services.AddServerSideBlazor().AddCircuitOptions(options => 
{ 
    options.DetailedErrors = true; 
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();
app.UseCssLiveReload();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();