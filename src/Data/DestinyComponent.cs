namespace TitleReport.Data
{
    public enum ComponentType
    {
        Records = 900,
        PresentationNodes = 700,
        Characters = 200,
    }

    // The component representing triumphs and seals
    // https://bungie-net.github.io/multi/schema_Destiny-Components-Records-DestinyProfileRecordsComponent.html#schema_Destiny-Components-Records-DestinyProfileRecordsComponent
    public class RecordsComponent
    {
        public int score { get; set; }

        public int activeScore { get; set; }

        public int legacyScore {get; set;}

        public int lifetimeScore {get; set;}

        public Dictionary<uint, RecordComponent> records {get; set;} = new Dictionary<uint, RecordComponent>();

        public uint recordCategoriesRootNodeHash {get; set;}

        public uint recordSealsRootNodeHash {get; set;}
    }

    //https://bungie-net.github.io/multi/schema_Destiny-Components-Records-DestinyRecordComponent.html#schema_Destiny-Components-Records-DestinyRecordComponent
    public class RecordComponent
    {
        public RecordState state {get; set;}

        public int? completedCount {get; set;}

        public ObjectiveProgress[] objectives { get; set; } = Array.Empty<ObjectiveProgress>();


    }

    public class ObjectiveProgress
    {
        public uint objectiveHash { get; set; }

        public uint destinationHash { get; set; }

        public uint activityHash { get; set; }

        public int? progress { get; set; }

        public int completionValue { get; set; }

        public bool complete { get; set; }

        public bool visible { get; set; }
    }

    public class PresentationNodesComponent
    {
        public Dictionary<uint, PresentationNodeComponent> nodes {get; set;}

        public PresentationNodesComponent()
        {
            nodes = new Dictionary<uint, PresentationNodeComponent>();
        }
    }

    public class PresentationNodeComponent
    {
        public int state { get; set; }

        public int progressValue { get; set; }

        public int completionValue { get; set; }

        public int? recordCategoryScore {get; set;}
    }

    /// <summary>
    /// https://bungie-net.github.io/multi/schema_Destiny-Misc-DestinyColor.html#schema_Destiny-Misc-DestinyColor
    /// </summary>
    public class DestinyColor
    {
        public byte red { get; set; }
        public byte green { get; set; }
        public byte blue { get; set; }
        public byte alpha { get; set; }
    }

    /// <summary>
    ///  https://bungie-net.github.io/multi/schema_Destiny-Entities-Characters-DestinyCharacterComponent.html#schema_Destiny-Entities-Characters-DestinyCharacterComponent
    /// </summary>
    public class CharacterComponent
    {
        public long membershipId { get; set; }

        public int membershipType { get; set; }

        public long characterId { get; set; }

        public DateTime dateLastPlayed { get; set; }
        public int light { get; set; }

        public string emblemPath { get; set; } = "";

        public string emblemBackgroundPath { get; set; } = "";

        public uint emblemHash { get; set; }

        public DestinyColor emblemColor { get; set; } = new();

        public uint titleRecordHash { get; set; }

    }
}