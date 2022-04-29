namespace TitleReport.Data
{
    public class BungieApiResponse<T>
    {
        public T? Response { get; set; }

        public int ErrorCode { get; set; }

        public int ThrottleSeconds { get; set; }

        public string ErrorStatus { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public object? MessageData { get; set; }

        public string DetailedErrorTrace { get; set; } = string.Empty;
    }

    public class ComponentObjectResponse<T>
    {

        public T data {get; set;}

        public int privacy {get; set;}

        public bool? disabled {get; set;}
    }

    public class BungieErrorStatus {
        public const string Success = "Success";
    }
}
