using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace api.Swagger
{
    public class RoleBasedDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var pathsToRemove = new List<string>();

            foreach (var path in swaggerDoc.Paths)
            {
                foreach (var operation in path.Value.Operations)
                {
                    // ← Obtenemos el descriptor del endpoint
                    if (context.ApiDescriptions.FirstOrDefault(a =>
                            a.RelativePath == path.Key.TrimStart('/')) is not { } apiDesc)
                        continue;

                    // Recuperar el método real
                    var actionDescriptor = apiDesc.ActionDescriptor as ControllerActionDescriptor;
                    if (actionDescriptor == null)
                        continue;

                    // ← Buscar si el endpoint tiene [Role(...)]
                    var roleAttr = actionDescriptor.MethodInfo
                        .GetCustomAttributes(typeof(RoleAttribute), false)
                        .Cast<RoleAttribute>()
                        .FirstOrDefault();

                    if (roleAttr != null)
                    {
                        operation.Value.Description +=
                            $"\n\n**Roles Permitidos:** {string.Join(", ", roleAttr.AllowedRoles)}";
                    }
                    else
                    {
                        operation.Value.Description += "\n\n**Roles Permitidos:** (no definido)";
                    }
                }
            }
        }
    }
}