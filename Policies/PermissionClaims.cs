using System;
using System.Collections.Generic;
using System.Reflection;

namespace SampleApi.Policies
{
    public static class PermissionClaims
    {
        public const string ReadUser = "user:read";
        public const string ReadUsers = "user:readAll";
        public const string CreateUser = "user:create";
        public const string UpdateUser = "user:update";
        public const string DeleteUser = "user:delete";

        public const string ReadRole = "role:read";
        public const string ReadRoles = "role:readAll";
        public const string CreateRole = "role:create";
        public const string UpdateRole = "role:update";
        public const string DeleteRole = "role:delete";

        public const string ReadItem = "item:read";
        public const string CreateItem = "item:create";
        public const string UpdateItem = "item:update";
        public const string DeleteItem = "item:delete";

        public static List<string> GetAll()
        {
            List<string> permissionClaimValues = new List<string>();
            Type type = typeof(PermissionClaims);
            foreach (var permissionClaim in type.GetFields())
            {
                permissionClaimValues.Add(permissionClaim.GetValue(null).ToString());
            }
            return permissionClaimValues;
        }
    }
}