namespace TitleReport.Data
{
    public class DestinyManifest
    {
        public string? version { get; set; }


        public Dictionary<string, LocalizedComponentContentPaths> jsonWorldComponentContentPaths { get; set; } = new Dictionary<string, LocalizedComponentContentPaths>();

    }

    public class LocalizedComponentContentPaths
    {
        public string? DestinyRecordDefinition { get; set; }

        public string? DestinyPresentationNodeDefinition { get; set;}

        public string? DestinyLoreDefinition { get; set; }

        public string? DestinyObjectiveDefinition { get; set; }

    }
}