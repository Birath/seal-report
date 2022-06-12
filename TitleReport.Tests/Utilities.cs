using Blazored.LocalStorage;
using BlazorDB;
using Bunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BungieSharper.Entities;
using BungieSharper.Entities.Destiny;
using BungieSharper.Entities.Destiny.Components.Records;
using BungieSharper.Entities.Destiny.Definitions.Common;
using BungieSharper.Entities.Destiny.Definitions.Presentation;
using BungieSharper.Entities.Destiny.Definitions.Records;
using BungieSharper.Entities.Destiny.Entities.Characters;
using BungieSharper.Entities.User;
using Newtonsoft.Json;
using TitleReport.Pages;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace TitleReport.Tests
{
    public static class Utilities
    {
        public static void SetupDefaultTestContext(TestContext ctx)
        {
            ctx.JSInterop.Mode = JSRuntimeMode.Strict;
            ctx.JSInterop.SetupVoid("window.blazorDB.createDb", _ => true).SetVoidResult();

            ctx.Services.AddBlazoredLocalStorage();
            ctx.AddBlazorDb(options =>
            {
                options.Name = Constants.DefinitionDataBaseName;
                options.StoreSchemas = new List<StoreSchema>()
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
                        Name = Constants.PresentationNodesStoreName,
                        PrimaryKey = "id",
                        PrimaryKeyAuto = true,
                        UniqueIndexes = new List<string> { "hash" },
                        Indexes = new List<string> { "nodeType" }
                    }
                };
            });
        }

        public static void PopulateDataBase(TestContext ctx)
        {
            var dbFactory = ctx.Services.GetService<IBlazorDbFactory>();
            var db = dbFactory.GetDbManager(Constants.DefinitionDataBaseName).Result;

            var presentationNodeText = File.ReadAllText("TestData/PresentationNodeDefinitions.json");
            var definitions =
                JsonSerializer.Deserialize<Dictionary<uint, DestinyPresentationNodeDefinition>>(presentationNodeText);
            db.BulkAddRecord(Constants.PresentationNodesStoreName, definitions!.Values);

            var recordsText = File.ReadAllText("TestData/RecordDefinitions.json");
            var recordDefinitions = JsonSerializer.Deserialize<Dictionary<uint, DestinyRecordDefinition>>(recordsText);
            db.BulkAddRecord(Constants.RecordStoreName, recordDefinitions!.Values);
        }

        public static UserInfoCard DefaultTestUser => new()
        {
            CrossSaveOverride = BungieMembershipType.TigerSteam,
            MembershipType = BungieMembershipType.TigerSteam,
            DisplayName = "TestUser0",
            BungieGlobalDisplayNameCode = 9999,
            BungieGlobalDisplayName = "TestUser0"
        };

        public static string DefaultUserName { get; } = "TestUser0#9999";

        public static DestinyRecordDefinition DefaultSealDefinition { get; } = new()
        {
            TitleInfo = new DestinyRecordTitleBlock()
            {
                TitlesByGender = new Dictionary<DestinyGender, string>
                {
                    { DestinyGender.Male, "Conqueror" }
                }
            },
            Hash = 3464275895,
        };

        public static DestinyPresentationNodeDefinition DefaultSealPresentationNode { get; } = new()
        {
            CompletionRecordHash = DefaultSealDefinition.Hash,
            DisplayProperties = new DestinyDisplayPropertiesDefinition()
            {
                Name = "Conqueror",
                Description = "Complete all Grandmaster Triumphs.",
            },
            OriginalIcon = "/common/destiny2_content/icons/d3548d7e67c29eaeb451549f7c7fa30f.png",
            ParentNodeHashes = new[]
            {
                UInt32.MaxValue,
            },
            NodeType = DestinyPresentationNodeType.Records,
            Children = new DestinyPresentationNodeChildrenBlock()
            {
                Records = new[]
                {
                    new DestinyPresentationNodeRecordChildEntry()
                    {
                        RecordHash = 0,
                        NodeDisplayPriority = 0
                    }
                }
            }
        };

        public static Seal DefaultSeal { get; } = new(
            "Conqueror",
            DefaultSealPresentationNode.DisplayProperties.Name,
            DefaultSealPresentationNode.DisplayProperties.Description,
            FilterProperty.Complete | FilterProperty.Legacy | FilterProperty.Gilded,
            $"{Constants.BungieManifestEndpoint}{DefaultSealPresentationNode.OriginalIcon}",
            Array.Empty<Triumph>()
        )
        {
            GildedCount = 4,
            IsGildedCurrentSeason = false,
        };

        public static Dictionary<uint, DestinyRecordComponent> DefaultRecordComponents => new()
        {
            {
                3464275895, new DestinyRecordComponent()
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
        };

        public static Dictionary<long, DestinyCharacterComponent> DefaultCharacterComponents => new()
        {
            {
                0, new DestinyCharacterComponent()
                {
                    DateLastPlayed = DateTime.Parse("2022-05-11T22:22:09Z"),
                    EmblemBackgroundPath = "/common/destiny2_content/icons/c612ec23a7f3b57042cb988788ef37a8.jpg",
                    TitleRecordHash = 3464275895,
                }
            },
            {
                1, new DestinyCharacterComponent()
                {
                    DateLastPlayed = DateTime.Parse("2022-03-15T19:04:59Z"),
                    EmblemBackgroundPath = "/common/destiny2_content/icons/c612ec23a7f3b57042cb988788ef37a8.jpg",
                }
            }
        };
    }
}