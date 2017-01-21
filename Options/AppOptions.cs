namespace SampleApi.Options
{
    public class AppOptions
    {
        public Application Application { get; set; }
        public ConnectionStrings ConnectionStrings { get; set; }
    }
    public class Application
    {
        public string Name { get; set; }
        public string Version { get; set; }
    }
    public class ConnectionStrings
    {
        public string MySqlProvider { get; set; }
    }
}