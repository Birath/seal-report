namespace TitleReport.Data
{
    public class DestinyProfile {

        public ComponentObjectResponse<RecordsComponent>? profileRecords { get; set; }         
        public ComponentObjectResponse<Dictionary<long, CharacterComponent>>? characters { get; set; }

    }
}