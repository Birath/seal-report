using Blazored.LocalStorage;
using BlazorDB;
using Bunit;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitleReport.Data;

namespace TitleReport.Tests
{
    public static class Utilities
    {
        public static void SetupDefaultTestContext(TestContext ctx)
        {
            ctx.JSInterop.Mode = JSRuntimeMode.Loose;

            ctx.Services.AddBlazoredLocalStorage();
            ctx.Services.AddBlazorDB(options =>
            {
                options.Name = Constants.DefinitionDataBaseName;
            });
        }

        public static UserInfoCard DefaultTestUser => new()
        {
            crossSaveOverride = 3,
            membershipType = 3,
            displayName = "TestUser0",
            bungieGlobalDisplayNameCode = 9999,
            bungieGlobalDisplayName = "TestUser0"
        };

        public static string DefaultUserName { get; } = "TestUser0#9999";
    }
}
