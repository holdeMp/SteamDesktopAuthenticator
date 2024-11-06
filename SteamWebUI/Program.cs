using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SteamWeb;
using SteamWebAuthenticator.Interfaces;
using SteamWebAuthenticator.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
builder.Services.AddScoped(sp => http);
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<IAccountService, AccountService>();
await builder.Build().RunAsync();