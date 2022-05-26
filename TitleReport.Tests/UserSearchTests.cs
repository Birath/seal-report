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
using BungieSharper.Entities;
using BungieSharper.Entities.User;
using TitleReport.Pages;

using Xunit;
using static TitleReport.Pages.SealSearch;

namespace TitleReport.Tests
{
    public class UserSearchTests
    {
        [Theory]
        [MemberData(nameof(Users))]
        public void SearchParsesUserCorretly(string name, UserInfoCard? expected)
        {
            using var ctx = new TestContext();

            Utilities.SetupDefaultTestContext(ctx);

            var mock = ctx.Services.AddMockHttpClient();

            mock.When(HttpMethod.Post, $"*/Destiny2/SearchDestinyPlayerByBungieName/-1/")
                .RespondJson(new ApiResponse<UserInfoCard?[]>
                {
                    Message = "Ok",
                    Response = expected is null ? Array.Empty<UserInfoCard>() : new[] { expected }
                });
            BungieName.TryParse(name, out var searchData);
            var searchComponent = ctx.RenderComponent<SealSearch>();
            var actual = searchComponent.Instance.SearchGuardianAsync(searchData!).Result;
            expected.ToExpectedObject().ShouldEqual(actual);
        }

        public static IEnumerable<object[]> Users => new List<object[]>
        {
            new object[] {"TestUser0#9999", Utilities.DefaultTestUser},
            new object[] {"NonExistitant#9999", null},
        };

        [Fact]
        public void SearchParsesMultipleMemberships()
        {
            using var ctx = new TestContext();

            Utilities.SetupDefaultTestContext(ctx);

            var mock = ctx.Services.AddMockHttpClient();
            var expected = Utilities.DefaultTestUser;
            var otherMemebershipUserData = Utilities.DefaultTestUser;
            otherMemebershipUserData.MembershipType = BungieMembershipType.TigerXbox;

            mock.When(HttpMethod.Post, $"*/Destiny2/SearchDestinyPlayerByBungieName/-1/")
                .RespondJson(new ApiResponse<UserInfoCard[]>
                {
                    Message = "Ok",
                    Response = new[] { Utilities.DefaultTestUser, otherMemebershipUserData }
                });
            BungieName.TryParse(Utilities.DefaultUserName, out var searchData);
            var searchComponent = ctx.RenderComponent<SealSearch>();
            var actual = searchComponent.Instance.SearchGuardianAsync(searchData!).Result;
            expected.ToExpectedObject().ShouldEqual(actual);
        }
    }
}
