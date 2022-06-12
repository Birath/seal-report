using System.Net;
using BungieSharper.Entities;
using BungieSharper.Entities.Destiny;
using BungieSharper.Entities.Destiny.Components.Presentation;
using BungieSharper.Entities.Destiny.Components.Records;
using BungieSharper.Entities.Destiny.Entities.Characters;
using BungieSharper.Entities.Destiny.Responses;
using BungieSharper.Entities.Exceptions;
using BungieSharper.Entities.User;
using Bunit;
using ExpectedObjects;
using RichardSzalay.MockHttp;
using TitleReport.Components.ProfileOverview;
using TitleReport.Pages;

namespace TitleReport.Tests;

public class SealSearchTests
{
    [Theory]
    [MemberData(nameof(Users))]
    public void SearchParsesUserCorrectly(string name, UserInfoCard? expected)
    {
        using var ctx = new TestContext();

        Utilities.SetupDefaultTestContext(ctx);

        var mock = ctx.Services.AddMockHttpClient();

        mock.When(HttpMethod.Post, $"*/Destiny2/SearchDestinyPlayerByBungieName/-1/")
            .RespondJson(new ApiResponse<UserInfoCard?[]>
            {
                ErrorCode = PlatformErrorCodes.Success,
                Message = "Ok",
                Response = expected is null ? Array.Empty<UserInfoCard>() : new[] { expected }
            });
        SealSearch.BungieName.TryParse(name, out var searchData);
        var searchComponent = ctx.RenderComponent<SealSearch>();
        var actual = searchComponent.Instance.SearchGuardianAsync(searchData!).Result;
        expected.ToExpectedObject().ShouldEqual(actual);
    }

    public static IEnumerable<object[]> Users => new List<object[]>
    {
        new object[] { "TestUser0#9999", Utilities.DefaultTestUser },
        new object[] { "NonExistitant#9999", null },
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
                ErrorCode = PlatformErrorCodes.Success,
                Message = "Ok",
                Response = new[] { Utilities.DefaultTestUser, otherMemebershipUserData }
            });
        SealSearch.BungieName.TryParse(Utilities.DefaultUserName, out var searchData);
        var searchComponent = ctx.RenderComponent<SealSearch>();
        var actual = searchComponent.Instance.SearchGuardianAsync(searchData!).Result;
        expected.ToExpectedObject().ShouldEqual(actual);
    }

    [Fact]
    public void SearchHandlesNonSuccessStatusCode()
    {
        var ctx = new TestContext();
        Utilities.SetupDefaultTestContext(ctx);
        var mock = ctx.Services.AddMockHttpClient();
        mock.When(HttpMethod.Post, $"*/Destiny2/SearchDestinyPlayerByBungieName/-1/")
            .Respond(HttpStatusCode.InternalServerError);

        SealSearch.BungieName.TryParse(Utilities.DefaultUserName, out var searchData);
        var searchComponent = ctx.RenderComponent<SealSearch>();
        var actual = searchComponent.Instance.SearchGuardianAsync(searchData!).Result;
        Assert.Null(actual);
    }

    [Fact]
    public void SearchHandlesBungieErrorResponse()
    {
        var ctx = new TestContext();
        Utilities.SetupDefaultTestContext(ctx);
        var mock = ctx.Services.AddMockHttpClient();
        mock.When(HttpMethod.Post, $"*/Destiny2/SearchDestinyPlayerByBungieName/-1/")
            .RespondJson(new ApiResponse<UserInfoCard[]>
            {
                ErrorCode = PlatformErrorCodes.BadRequest,
                ErrorStatus = "ERROR",
                Message = "Bad Request",
                Response = Array.Empty<UserInfoCard>()
            });

        SealSearch.BungieName.TryParse(Utilities.DefaultUserName, out var searchData);
        var searchComponent = ctx.RenderComponent<SealSearch>();
        var actual = searchComponent.Instance.SearchGuardianAsync(searchData!).Result;
        Assert.Null(actual);
    }

    [Theory]
    [InlineData("{\"Message\": }")]
    [InlineData("\"\"")]
    public void SearchHandlesInvalidJsonResponse(string response)
    {
        var ctx = new TestContext();
        Utilities.SetupDefaultTestContext(ctx);
        var mock = ctx.Services.AddMockHttpClient();
        mock.When(HttpMethod.Post, $"*/Destiny2/SearchDestinyPlayerByBungieName/-1/")
            .Respond(new StringContent(response));

        SealSearch.BungieName.TryParse(Utilities.DefaultUserName, out var searchData);
        var searchComponent = ctx.RenderComponent<SealSearch>();
        var actual = searchComponent.Instance.SearchGuardianAsync(searchData!).Result;
        Assert.Null(actual);
    }

    [Theory]
    [MemberData(nameof(RecordComponentData))]
    public void ParsesSealCorrectly(Dictionary<uint, DestinyRecordComponent> recordsComponents, List<Seal> expected)
    {
        using var ctx = new TestContext();

        Utilities.SetupDefaultTestContext(ctx);
        Utilities.PopulateDataBase(ctx);

        var searcher = ctx.RenderComponent<SealSearch>();

        searcher.WaitForState(() => searcher.Instance.DataBaseManager != null);
        var actual = searcher.Instance.GetUserSeals(new DestinyProfileRecordsComponent()
        {
            Records = recordsComponents,
            RecordSealsRootNodeHash = uint.MaxValue,
        }).Result.ToList();

        expected.ToExpectedObject().ShouldMatch(actual);
    }

    public static IEnumerable<object[]> RecordComponentData => new[]
    {
        new object[]
        {
            Utilities.DefaultRecordComponents,
            new[] { Utilities.DefaultSeal }
        },
        new object[]
        {
        }
    };

    [Theory]
    [MemberData(nameof(CharacterData))]
    public void DestinyCharacterParsedCorrectly(string name, IEnumerable<DestinyCharacterComponent> characters,
        ProfileOverviewData expected)
    {
        using var ctx = new TestContext();

        Utilities.SetupDefaultTestContext(ctx);
        Utilities.PopulateDataBase(ctx);
        var searchComponent = ctx.RenderComponent<SealSearch>();
        searchComponent.WaitForState(() => searchComponent.Instance.DataBaseManager != null);
        var actual = searchComponent.Instance.GetUserProfileOverview(name, characters,
            new DestinyProfileRecordsComponent()
            {
                Records = new Dictionary<uint, DestinyRecordComponent>
                {
                    {
                        Utilities.DefaultSealDefinition.Hash, new DestinyRecordComponent
                        {
                            CompletedCount = Utilities.DefaultSeal.GildedCount,
                            State = DestinyRecordState.CanEquipTitle
                        }
                    },
                    {
                        1715149073, new DestinyRecordComponent() // Gilding record
                        {
                            CompletedCount = Utilities.DefaultSeal.GildedCount,
                            State = DestinyRecordState.ObjectiveNotCompleted
                        }
                    }
                }
            }).Result;

        expected.ToExpectedObject().ShouldEqual(actual);
    }

    public static IEnumerable<object[]> CharacterData => new List<object[]>
    {
        new object[]
        {
            Utilities.DefaultUserName,
            Utilities.DefaultCharacterComponents.Values,
            new ProfileOverviewData(
                Utilities.DefaultUserName,
                "https://www.bungie.net/common/destiny2_content/icons/c612ec23a7f3b57042cb988788ef37a8.jpg",
                Utilities.DefaultSeal
            )
        },
    };

    [Fact]
    public void SearchCreatesCorrectResult()
    {
        using var ctx = new TestContext();

        Utilities.SetupDefaultTestContext(ctx);
        var mock = ctx.Services.AddMockHttpClient();
        Utilities.PopulateDataBase(ctx);

        mock.When(HttpMethod.Post, $"*/Destiny2/SearchDestinyPlayerByBungieName/-1/")
            .RespondJson(new ApiResponse<UserInfoCard?[]>
            {
                ErrorCode = PlatformErrorCodes.Success,
                Message = "Ok",
                Response = new[] { Utilities.DefaultTestUser }
            });
        mock.When(HttpMethod.Get, $"*/Platform/Destiny2/TigerSteam/Profile/0/*")
            .RespondJson(new ApiResponse<DestinyProfileResponse>
            {
                ErrorCode = PlatformErrorCodes.Success,
                Message = "Ok",
                Response = new DestinyProfileResponse()
                {
                    ProfileRecords = new SingleComponentResponseOfDestinyProfileRecordsComponent()
                    {
                        Data = new DestinyProfileRecordsComponent()
                        {
                            Records = Utilities.DefaultRecordComponents
                        }
                    },
                    Characters = new DictionaryComponentResponseOfint64AndDestinyCharacterComponent()
                    {
                        Data = Utilities.DefaultCharacterComponents,
                    },
                    ProfilePresentationNodes = new SingleComponentResponseOfDestinyPresentationNodesComponent()
                    {
                        Data = new DestinyPresentationNodesComponent()
                        {
                            Nodes = new Dictionary<uint, DestinyPresentationNodeComponent>()
                        }
                    }
                    
                }
            });

        var searchComponent = ctx.RenderComponent<SealSearch>();
        var inputField = searchComponent.Find("fluent-text-field");
        inputField.SetAttribute("current-value", Utilities.DefaultUserName);
        inputField.Change(Utilities.DefaultUserName);
        searchComponent.SaveSnapshot();
        searchComponent.Find("fluent-button").Click();

        searchComponent.WaitForAssertion(() => Assert.Equal(searchComponent.Instance.ActiveProfile?.UserName, Utilities.DefaultTestUser.DisplayName));

        var diffs = searchComponent.GetChangesSinceSnapshot();
        // diffs.ShouldHaveChanges(diff => diff.ShouldBeAddition());
    }
}