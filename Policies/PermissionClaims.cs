namespace SampleApi.Policies
{
    public static class PermissionClaims
    {
        public const string ReadUser = "user:read";
        public const string CreateUser = "user:create";
        public const string UpdateUser = "user:update";
        public const string DeleteUser = "user:delete";

        public const string ReadItem = "item:read";
        public const string CreateItem = "item:create";
        public const string UpdateItem = "item:update";
        public const string DeleteItem = "item:delete";

    }
}