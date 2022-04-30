namespace TitleReport.Data
{
    public class PresentationChildBlock
    {
        public int presentationNodeType { get; set; }

        public uint[]? parentPresentationNodeHashes { get; set; }

        public int displayStyle { get; set; }
    }

    public class PresentationNodeChildrenBlock
    {
        public PresentationNodeChildEntry[] presentationNodes { get; set; } = Array.Empty<PresentationNodeChildEntry>(); 

        public PresentationNodeCollectibleChildEntry[] collectibles { get; set; } = Array.Empty<PresentationNodeCollectibleChildEntry>();

        public PresentationNodeRecordChildEntry[] records { get; set; } = Array.Empty<PresentationNodeRecordChildEntry>();

        public PresentationNodeMetricChildEntry[] metrics { get; set; } = Array.Empty<PresentationNodeMetricChildEntry>(); 

        public PresentationNodeCraftableChildEntry[] craftables { get; set; } = Array.Empty<PresentationNodeCraftableChildEntry>(); 
    }

    public abstract class ChildEntry
    { 
        public uint nodeDisplayPriority { get; set; }
    }


    public class PresentationNodeChildEntry : ChildEntry
    {
        public uint presentationNodeHash { get; set; }
    }

    public class PresentationNodeCollectibleChildEntry : ChildEntry
    {
        public uint collectibleHash { get; set; }
    }

    public class PresentationNodeRecordChildEntry : ChildEntry
    {
        public uint recordHash { get; set; }
    }

    public class PresentationNodeMetricChildEntry : ChildEntry
    {
        public uint metricHash { get; set; }
    }

    public class PresentationNodeCraftableChildEntry : ChildEntry
    {
        public uint craftableItemHash { get; set; }
    }
}