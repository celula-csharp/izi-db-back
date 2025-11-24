using System;

namespace api.Swagger
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RoleAttribute : Attribute
    {
        public string[] AllowedRoles { get; }

        public RoleAttribute(params string[] roles)
        {
            AllowedRoles = roles;
        }
    }
}