using BlazorDB;

using Bunit;

using ExpectedObjects;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TitleReport.Components.ProfileOverview;
using TitleReport.Data;
using TitleReport.Pages;

namespace TitleReport.Tests
{
    public class CharacterParsingTests
    {
        [Theory]
        [MemberData(nameof(CharacterData))]
        public void DestinyCharacterParsedCorrectly(string name, IEnumerable<CharacterComponent> characters, ProfileOverviewData expected)
        {
            using var ctx = new TestContext();

            Utilities.SetupDefaultTestContext(ctx);

            var a = ctx.JSInterop.Setup<RecordDefinition>($"window.blazorDB.findItemByIndex", (invocation) =>
            {
                dynamic data = invocation.Arguments[2]!;
                var filter = (IndexFilterValue)data.GetType().GetProperties()[2].GetValue(data);
                return filter.FilterValue as uint? == 3464275895;
            }).SetResult(Utilities.DefaultSealDefinition);

            ctx.JSInterop.Setup<IList<PresentationNodeDefinition>>("window.blazorDB.where", (invocation) =>
            {
                var data = ((List<IndexFilterValue>)invocation.Arguments[4]!)[0].IndexName!;
                return data == "nodeType";
            }).SetResult(new List<PresentationNodeDefinition>
            {
                Utilities.DefaultSealPresentationNode
            });

            ctx.JSInterop.SetupVoid("window.blazorDB.createDb", _ => true).SetVoidResult();
            
            var call = ctx.JSInterop.Setup<PresentationNodeDefinition>("window.blazorDB.findItemByIndex");
            call.SetResult(new PresentationNodeDefinition());

            var mock = ctx.Services.AddMockHttpClient();
            
            var searchComponent = ctx.RenderComponent<SealSearch>();
            searchComponent.WaitForState(() => searchComponent.Instance.DataBaseManager != null);
            var actual = searchComponent.Instance.GetUserProfileOverview(name, characters, new RecordsComponent()
            {
                records = new Dictionary<uint, RecordComponent>
                {
                    { Utilities.DefaultSealDefinition.hash, new RecordComponent 
                        {
                            completedCount = 1,
                            state = RecordState.CanEquipTitle
                        }
                    }
                }
            }).Result;
            
            expected.ToExpectedObject().ShouldEqual(actual);
        }

        public static IEnumerable<object[]> CharacterData => new List<object[]>
        {
            new object[] {
                Utilities.DefaultUserName, 
                new CharacterComponent[] {
                    new CharacterComponent() {
                        dateLastPlayed = DateTime.Parse("2022-05-11T22:22:09Z"),
                        emblemBackgroundPath = "https://bungie.net/common/destiny2_content/icons/c612ec23a7f3b57042cb988788ef37a8.jpg",
                        titleRecordHash = 3464275895,
                    },
                    new CharacterComponent() {
                        dateLastPlayed = DateTime.Parse("2022-03-15T19:04:59Z"),
                        emblemBackgroundPath = "https://bungie.net/common/destiny2_content/icons/c612ec23a7f3b57042cb988788ef37a8.jpg",
                    }
                },
                new ProfileOverviewData(
                    Utilities.DefaultUserName,
                    "https://bungie.net/common/destiny2_content/icons/c612ec23a7f3b57042cb988788ef37a8.jpg", 
                    Utilities.DefaultSeal
                )
            },
        };

    }
}
