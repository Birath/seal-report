using Blazored.LocalStorage;
using BlazorDB;
using Bunit;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitleReport.Data;
using TitleReport.Pages;

namespace TitleReport.Tests
{
    public static class Utilities
    {
        public static void SetupDefaultTestContext(TestContext ctx)
        {
            ctx.JSInterop.Mode = JSRuntimeMode.Strict;

            ctx.Services.AddBlazoredLocalStorage();
            ctx.Services.AddBlazorDB(options =>
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
            crossSaveOverride = 3,
            membershipType = 3,
            displayName = "TestUser0",
            bungieGlobalDisplayNameCode = 9999,
            bungieGlobalDisplayName = "TestUser0"
        };

        public static string DefaultUserName { get; } = "TestUser0#9999";

        public static RecordDefinition DefaultSealDefinition { get; } = new()
        {
            titleInfo = new TitleInfo()
            {
                titlesByGender = new Dictionary<string, string>
                    {
                        { "Male", "Conqueror"}
                    }
            },
            hash = 3464275895,
        };

        public static PresentationNodeDefinition DefaultSealPresentationNode { get; } = new()
        {
            completionRecordHash = DefaultSealDefinition.hash,
            displayProperties = new DisplayPropertiesDefinition()
            {
                name = "Conqueror",
                description = "Complete all Grandmaster Triumphs.",
            },
            originalIcon = "/common/destiny2_content/icons/d3548d7e67c29eaeb451549f7c7fa30f.png"
        };
        public static Seal DefaultSeal { get; } = new Seal(
            "Conqueror", 
            DefaultSealPresentationNode.displayProperties.name, 
            DefaultSealPresentationNode.displayProperties.description, 
            FilterProperty.Complete | FilterProperty.Legacy,
            $"{Constants.BungieManifestEndpoint}{DefaultSealPresentationNode.originalIcon}", 
            Array.Empty<Triumph>()
        );

    }
}
