namespace TitleReport.Data
{

    [Flags]
    // https://bungie-net.github.io/multi/schema_Destiny-DestinyRecordState.html#schema_Destiny-DestinyRecordState
    public enum RecordState : int
    {
        None = 0,
        RecordRedeemed = 1,
        RewardUnavailable = 2,
        ObjectiveNotCompleted = 4,
        Obscured = 8,
        Invisible = 16,
        EntitlementUnowned = 32,
        CanEquipTitle = 64,
    }

    // https://bungie-net.github.io/multi/schema_Destiny-DestinyPresentationNodeType.html#schema_Destiny-DestinyPresentationNodeType
    public enum PresentationNodeType
    {
        Default = 0,
        Category = 1,
        Collectibles = 2,
        Records = 3,
        Metric = 4,
        Craftable = 5,
    }
}
