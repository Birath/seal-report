using TitleReport.Pages;

namespace TitleReport.Components.ProfileOverview
{
    public class ProfileOverviewData
    {
        public string UserName { get; }
        public string EmblemUrl { get; }
        public Seal EquipedSeal { get;}
        public List<Seal> Seals { get; } = new List<Seal>();

        public ProfileOverviewData(string userName, string emblemUrl, Seal equipedSeal)
        {
            UserName = userName;
            EmblemUrl = emblemUrl;
            EquipedSeal = equipedSeal;
        }
    }
}