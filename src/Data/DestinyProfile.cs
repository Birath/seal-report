namespace TitleReport.Data
{
    public class DestinyProfile {

        public ComponentObjectResponse<RecordsComponent>? profileRecords { get; set; }         
        public ComponentObjectResponse<CharacterComponent>? characters { get; set; }

    }
}