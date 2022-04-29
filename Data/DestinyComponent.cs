namespace TitleReport.Data
{
    public enum ComponentType
    {
        Records = 900,
        PresentationNodes = 700,
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
}