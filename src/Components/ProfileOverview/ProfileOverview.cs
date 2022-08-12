using TitleReport.Pages;

namespace TitleReport.Components.ProfileOverview
{
    public class ProfileOverviewData
    {
        public string UserName { get; }
        public short? DisplayNameCode { get; init; }
        public string EmblemUrl { get; }
        public Seal? EquippedSeal { get;}
        public List<Seal> Seals { get; } = new List<Seal>();

        public ProfileOverviewData(string userName, string emblemUrl, Seal? equippedSeal)
        {
            UserName = userName;
            EmblemUrl = emblemUrl;
            EquippedSeal = equippedSeal;
        }
    }
}