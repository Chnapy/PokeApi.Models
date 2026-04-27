using NJsonSchema;

public class PascalCaseTypeNameGenerator : ITypeNameGenerator
{
    public string Generate(JsonSchema schema, string? typeNameHint, IEnumerable<string> reservedTypeNames)
    {
        var pathParts = GetDocumentPath(schema)?.Split('/') ?? [];

        if (pathParts.Any(part => part.StartsWith('$') && part != "$id"))
        {
            throw new Exception($"Path parameter with '$' not handled: {schema.DocumentPath}");
        }

        var finalName = BuildTypeNameFromPath(string.Join('/', pathParts));
        finalName = finalName.Length > 0
            ? finalName
            : "PokeApi";
        // Console.WriteLine($"{finalName} - {ToPascalCase(typeNameHint)} - {schema.DocumentPath != null}");

        if (schema.DocumentPath == null)
            return finalName + ToPascalCase(typeNameHint ?? "");

        return finalName;
    }

    private static string? GetDocumentPath(JsonSchema schema)
    {
        if (schema.DocumentPath == null)
        {
            return schema.ParentSchema != null
                ? GetDocumentPath(schema.ParentSchema)
                : null;
        }

        var pathParts = schema.DocumentPath
            .Replace('\\', '/')
            .Split('/')
            .Where(s => s.Trim().Length > 0)
            .ToArray();

        if (pathParts.Length >= 2 && pathParts[0] == "pokeapi-data" && pathParts[1] == "data")
        {
            pathParts = [.. pathParts.Skip(2)];
        }

        if (pathParts.Length >= 2 && pathParts[0] == "schema" && pathParts[1] == "v2")
        {
            pathParts = [.. pathParts.Skip(2)];
        }

        return string.Join('/', pathParts);
    }

    private static string BuildTypeNameFromPath(string path)
    {
        var segments = path
            .Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                   StringSplitOptions.RemoveEmptyEntries)
            .Where(s => s != "$id" && s != "index.json")
            .Select((s, i) =>
            {
                return s.Contains('.') ? Path.GetFileNameWithoutExtension(s) : s;
            })
            .Where(s => !string.IsNullOrWhiteSpace(s));

        return string.Concat(segments.Select(ToPascalCase));
    }

    public static string ToPascalCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        // Split on non-alpha chars (_, -, space, etc.)
        // keep numbers
        var words = System.Text.RegularExpressions.Regex
            .Split(input, @"[^a-zA-Z0-9]+")
            .Where(w => w.Length > 0);

        return string.Concat(words.Select(Capitalize));
    }

    private static string Capitalize(string word) =>
        char.ToUpperInvariant(word[0]) + word[1..];
}
