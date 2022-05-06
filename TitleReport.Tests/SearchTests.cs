using Blazored.LocalStorage;
using BlazorDB;
using Bunit;
using ExpectedObjects;
using RichardSzalay.MockHttp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TitleReport.Data;
using TitleReport.Pages;

using Xunit;

using static TitleReport.Pages.SealSearch;

namespace TitleReport.Tests
{
    public class SearchTests
    {
        [Fact]
        public void SearchParsesUserCorretly()
        {
            using var ctx = new TestContext();

            SetupDefaultTestContext(ctx);

            var mock = ctx.Services.AddMockHttpClient();

            BungieName.TryParse("TestUser0#9999", out var serachData);
            var expected = new UserInfoCard
            {
                crossSaveOverride = 3,
                membershipType = 3,
                displayName = serachData!.displayName,
                bungieGlobalDisplayNameCode = (short)serachData.displayNameCode,
                bungieGlobalDisplayName = "TestUser0"
            };
            mock.When(HttpMethod.Post, $"*/Destiny2/SearchDestinyPlayerByBungieName/-1/")
                .RespondJson(new BungieApiResponse<UserInfoCard[]>
            {
                Message = "Ok",
                Response = new[] { expected }
            });
            var searchComponent = ctx.RenderComponent<SealSearch>();
            var actual = searchComponent.Instance.SearchGuardianAsync(serachData).Result;
            expected.ToExpectedObject().ShouldEqual(actual);
        }

        [Fact]
        public void SearchReturnsNullWhenInvalidName()
        {
            using var ctx = new TestContext();

            SetupDefaultTestContext(ctx);

            var mock = ctx.Services.AddMockHttpClient();
            BungieName.TryParse("NonExistitant#9999", out var serachData);
 
            mock.When(HttpMethod.Post, $"*/Destiny2/SearchDestinyPlayerByBungieName/-1/")
                .RespondJson(new BungieApiResponse<UserInfoCard[]>
                {
                    Message = "Ok",
                    Response = Array.Empty<UserInfoCard>()
                });
            var searchComponent = ctx.RenderComponent<SealSearch>();
            
            var actual = searchComponent.Instance.SearchGuardianAsync(serachData!).Result;
            Assert.Null(actual);
        }

        private static void SetupDefaultTestContext(TestContext ctx)
        {
            ctx.JSInterop.Mode = JSRuntimeMode.Loose;

            ctx.Services.AddBlazoredLocalStorage();
            ctx.Services.AddBlazorDB(options =>
            {
                options.Name = Constants.DefinitionDataBaseName;
            });
        }

    }
}
