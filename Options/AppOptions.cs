using System;
using System.Collections.Generic;

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
        public List<string> Audiences { get; set; }
        public string SecretKey { get; set; }
        public int AccessTokenLifetime { get; set; }
        public int IdentityTokenLifetime { get; set; }
        public int RefreshTokenLifetime { get; set; }
    }
}