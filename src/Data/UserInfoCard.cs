namespace TitleReport.Data
{
    // https://bungie-net.github.io/multi/schema_User-UserInfoCard.html#schema_User-UserInfoCard
    public class UserInfoCard
    {
        public string supplementalDisplayName { get; set; } = "";

        public string iconPath { get; set; } = "";

        public int crossSaveOverride { get; set; }

        public int[] applicableMembershipTypes { get; set; } = Array.Empty<int>();

        public bool isPublic { get; set; }

        public int membershipType { get; set; }

        public long membershipId { get; set; }

        public string displayName { get; set; } = "";

        public string bungieGlobalDisplayName { get; set; } = "";

        public short? bungieGlobalDisplayNameCode { get; set; }
    }


}
