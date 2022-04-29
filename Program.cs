using BlazorDB;

using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Fast.Components.FluentUI;

using TitleReport;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddFluentUIComponents();
builder.Services.AddBlazorDB(options =>
{
    options.Name = Constants.DefinitionDataBaseName;
    options.Version = 3;
    options.StoreSchemas = new List<StoreSchema>
    {
        new StoreSchema
        {
            Name = Constants.RecordStoreName,
            PrimaryKey = "id",
            PrimaryKeyAuto = true,
            UniqueIndexes = new List<string> { "hash" }
        },
        new StoreSchema
        {
            Name = Constants.LoreStoreName,
            PrimaryKey = "id",
            PrimaryKeyAuto = true,
            UniqueIndexes = new List<string> { "hash" }
        },
        new StoreSchema
        {
            Name = Constants.ObjectivesStoreName,
            PrimaryKey = "id",
            PrimaryKeyAuto = true,
            UniqueIndexes = new List<string> { "hash" }
        },
        new StoreSchema
        {
            Name = Constants.PresentationNodesStoreName,
            PrimaryKey = "id",
            PrimaryKeyAuto = true,
            UniqueIndexes = new List<string> { "hash" },
            Indexes = new List<string> { "nodeType" }
        }

    };
});
var host = builder.Build();

await host.RunAsync();
