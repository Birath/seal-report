using System.Text.Json.Serialization;

namespace TitleReport.Data
{

    public abstract class BaseDestinyDefinition
    {
        public DisplayPropertiesDefinition displayProperties { get; set; }

        public uint hash { get; set; }

        public int index { get; set; }

        public bool redacted { get; set; }

        public BaseDestinyDefinition()
        {
            displayProperties = new DisplayPropertiesDefinition();
        }
    }

    // https://bungie-net.github.io/multi/schema_Destiny-Definitions-Records-DestinyRecordDefinition.html#schema_Destiny-Definitions-Records-DestinyRecordDefinition
    public class RecordDefinition : BaseDestinyDefinition
    {

        public int? scope { get; set; }

        public PresentationChildBlock? presentationInfo { get; set; }

        public uint? loreHash { get; set; }

        public uint[] objectiveHashes { get; set; } = Array.Empty<uint>();

        public int? recordValueStyle { get; set; }

        public bool? forTitleGilding { get; set; }

        public TitleInfo titleInfo { get; set; } = new TitleInfo();

        public CompletionInfo? completionInfo { get; set; }

        public ExpirationInfo? expirationInfo { get; set; }
    }

    // https://bungie-net.github.io/multi/schema_Destiny-Definitions-Common-DestinyDisplayPropertiesDefinition.html#schema_Destiny-Definitions-Common-DestinyDisplayPropertiesDefinition
    public class DisplayPropertiesDefinition
    {
        public string description { get; set; } = "";

        public string name { get; set; } = "";

        public string icon { get; set; } = "";

        public IconSequenceDefinition[]? iconSequences { get; set; }

        public string? highResIcon { get; set; }

        public bool hasIcon { get; set; }
    }

    public class IconSequenceDefinition
    {
        public string[]? frames { get; set; }
    }

    public class TitleInfo
    {
        public bool hasTitle { get; set; }

        public Dictionary<string, string> titlesByGender { get; set; } = new Dictionary<string, string>();

        public Dictionary<uint, string> titlesByGenderHash { get; set; } = new Dictionary<uint, string>();

        public uint? gildingTrackingRecordHash { get; set; }

    }

    public class CompletionInfo
    {
        public int? partialCompletionObjectiveCountThreshold { get; set; }

        public int? ScoreValue { get; set; }
    }

    public class ExpirationInfo
    {
        public bool? hasExpiration { get; set; }

        public string? description { get; set; }

        public string? icon { get; set; }

    }

    // https://bungie-net.github.io/multi/schema_Destiny-Definitions-Presentation-DestinyPresentationNodeDefinition.html#schema_Destiny-Definitions-Presentation-DestinyPresentationNodeDefinition
    public class PresentationNodeDefinition : BaseDestinyDefinition
    {
        public string? originalIcon { get; set; }

        public string? rootViewIcon { get; set; }


        public PresentationNodeType nodeType { get; set; }

        public uint? completionRecordHash { get; set; }

        public uint[] parentNodeHashes { get; set; } = Array.Empty<uint>(); 

        public PresentationNodeChildrenBlock children { get; set; } = new PresentationNodeChildrenBlock();

        public PresentationNodeType presentationNodeType { get; set; }

    }

    // https://bungie-net.github.io/multi/schema_Destiny-Definitions-Lore-DestinyLoreDefinition.html#schema_Destiny-Definitions-Lore-DestinyLoreDefinition
    public class LoreDefinition : BaseDestinyDefinition
    {

        public string? subtitle { get; set; }

        public LoreDefinition()
        {
            displayProperties = new DisplayPropertiesDefinition();
        }
    }

    public class ObjectiveDefinition : BaseDestinyDefinition
    {

    }


}
