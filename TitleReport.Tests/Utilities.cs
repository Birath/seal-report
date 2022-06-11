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
using BungieSharper.Entities.Destiny.Definitions.Common;
using BungieSharper.Entities.Destiny.Definitions.Presentation;
using BungieSharper.Entities.Destiny.Definitions.Records;
using BungieSharper.Entities.User;
using TitleReport.Pages;

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
                        { DestinyGender.Male, "Conqueror"}
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
            ParentNodeHashes = new []
            {
                UInt32.MaxValue, 
            }
        };
        public static Seal DefaultSeal { get; } = new Seal(
            "Conqueror", 
            DefaultSealPresentationNode.DisplayProperties.Name, 
            DefaultSealPresentationNode.DisplayProperties.Description, 
            FilterProperty.Complete | FilterProperty.Legacy,
            $"{Constants.BungieManifestEndpoint}{DefaultSealPresentationNode.OriginalIcon}", 
            Array.Empty<Triumph>()
        );

    }
}
