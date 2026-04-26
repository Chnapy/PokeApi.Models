using NJsonSchema;
using NJsonSchema.References;

/// <summary>
/// Solve $ref of PokeApi schemas.
/// </summary>
public class PokeApiReferenceResolver : JsonReferenceResolver
{
    private readonly string _schemaRoot;

    public PokeApiReferenceResolver(JsonSchema rootSchema, string schemaRoot)
        : base(new JsonSchemaAppender(rootSchema, new DefaultTypeNameGenerator()))
    {
        _schemaRoot = schemaRoot.TrimEnd(Path.DirectorySeparatorChar, '/');
    }

    public override Task<IJsonReference> ResolveFileReferenceAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        if (filePath.Length > 2 && filePath[1] == ':')
        {
            filePath = filePath[2..];
        }

        if (filePath.StartsWith('/') || filePath.StartsWith('\\'))
        {
            filePath = filePath[1..];
        }

        var absoluteFilePath = Path.Combine(_schemaRoot, filePath);

        return base.ResolveFileReferenceAsync(absoluteFilePath, cancellationToken);
    }
}
