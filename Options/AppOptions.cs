namespace SampleApi.Options
{
    public class AppOptions
    {
        public Application Application { get; set; }
        public ConnectionStrings ConnectionStrings { get; set; }
        public Jwt Jwt { get; set; }
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
    public class Jwt
    {
        public string Audience { get; set; }
        public string Authority { get; set; }
    }
}