using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace api.Swagger;

public class RoleOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Extraer atributo [Authorize(Role="admin")]
        var roleAttribute = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<RoleRequirementAttribute>()
            .FirstOrDefault();

        if (roleAttribute != null)
        {
            operation.Extensions.Add("x-roles", new Microsoft.OpenApi.Any.OpenApiString(roleAttribute.Role));
        }
    }
}

// Atributo personalizado
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class RoleRequirementAttribute : Attribute
{
    public string Role { get; }

    public RoleRequirementAttribute(string role)
    {
        Role = role;
    }
}