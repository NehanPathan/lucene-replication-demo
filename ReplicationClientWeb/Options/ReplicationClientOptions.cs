namespace ReplicationClientWeb.Options
{
    public class ReplicationClientOptions
    {
        public string ServerUrl { get; set; } = string.Empty;
        public string IndexPath { get; set; } = string.Empty;
        public string TempPath { get; set; } = string.Empty;

        public TimeSpan PullInterval { get; set; } = TimeSpan.FromSeconds(10); 

    }
}
