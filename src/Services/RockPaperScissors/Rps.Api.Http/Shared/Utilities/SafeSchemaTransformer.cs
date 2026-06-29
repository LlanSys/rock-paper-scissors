using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

using Serilog;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rps.Api.Http.Shared.Utilities;

internal sealed class SafeSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        try
        {
            if (schema == null) return Task.CompletedTask;

            if (schema.Properties != null)
            {
                var problematicKeys = schema.Properties
                    .Where(kvp => string.IsNullOrWhiteSpace(kvp.Key) || kvp.Value == null)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in problematicKeys)
                {
                    schema.Properties.Remove(key);
                }
            }

            if (schema.AdditionalPropertiesAllowed && schema.AdditionalProperties != null)
            {
                if (schema.AdditionalProperties.Type == JsonSchemaType.Null || schema.AdditionalProperties.Type == default)
                {
                    schema.AdditionalProperties = null;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Error in schema transformation for type {TypeName}", context?.JsonTypeInfo?.Type?.Name);
        }

        return Task.CompletedTask;
    }
}
