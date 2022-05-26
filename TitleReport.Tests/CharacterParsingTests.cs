using BlazorDB;

using Bunit;

using ExpectedObjects;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BungieSharper.Entities.Destiny;
using BungieSharper.Entities.Destiny.Components.Records;
using BungieSharper.Entities.Destiny.Definitions.Presentation;
using BungieSharper.Entities.Destiny.Definitions.Records;
using BungieSharper.Entities.Destiny.Entities.Characters;
using TitleReport.Components.ProfileOverview;
using TitleReport.Pages;

namespace TitleReport.Tests
{
    public class CharacterParsingTests
    {
        [Theory]
        [MemberData(nameof(CharacterData))]
        public void DestinyCharacterParsedCorrectly(string name, IEnumerable<DestinyCharacterComponent> characters, ProfileOverviewData expected)
        {
            using var ctx = new TestContext();

            Utilities.SetupDefaultTestContext(ctx);

            var a = ctx.JSInterop.Setup<DestinyRecordDefinition>($"window.blazorDB.findItemByIndex", (invocation) =>
            {
                dynamic data = invocation.Arguments[2]!;
                var filter = (IndexFilterValue)data.GetType().GetProperties()[2].GetValue(data);
                return filter.FilterValue as uint? == 3464275895;
            }).SetResult(Utilities.DefaultSealDefinition);

            ctx.JSInterop.Setup<IList<DestinyPresentationNodeDefinition>>("window.blazorDB.where", (invocation) =>
            {
                var data = ((List<IndexFilterValue>)invocation.Arguments[4]!)[0].IndexName!;
                return data == "nodeType";
            }).SetResult(new List<DestinyPresentationNodeDefinition>
            {
                Utilities.DefaultSealPresentationNode
            });

            var call = ctx.JSInterop.Setup<DestinyPresentationNodeDefinition>("window.blazorDB.findItemByIndex");
            call.SetResult(new DestinyPresentationNodeDefinition());
            
            var searchComponent = ctx.RenderComponent<SealSearch>();
            searchComponent.WaitForState(() => searchComponent.Instance.DataBaseManager != null);
            var actual = searchComponent.Instance.GetUserProfileOverview(name, characters, new DestinyProfileRecordsComponent()
            {
                Records = new Dictionary<uint, DestinyRecordComponent>
                {
                    { Utilities.DefaultSealDefinition.Hash, new DestinyRecordComponent 
                        {
                            CompletedCount = 1,
                            State = DestinyRecordState.CanEquipTitle
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
                new[] {
                    new DestinyCharacterComponent() {
                        DateLastPlayed = DateTime.Parse("2022-05-11T22:22:09Z"),
                        EmblemBackgroundPath = "/common/destiny2_content/icons/c612ec23a7f3b57042cb988788ef37a8.jpg",
                        TitleRecordHash = 3464275895,
                    },
                    new DestinyCharacterComponent() {
                        DateLastPlayed = DateTime.Parse("2022-03-15T19:04:59Z"),
                        EmblemBackgroundPath = "/common/destiny2_content/icons/c612ec23a7f3b57042cb988788ef37a8.jpg",
                    }
                },
                new ProfileOverviewData(
                    Utilities.DefaultUserName,
                    "https://www.bungie.net/common/destiny2_content/icons/c612ec23a7f3b57042cb988788ef37a8.jpg", 
                    Utilities.DefaultSeal
                )
            },
        };

    }
}
