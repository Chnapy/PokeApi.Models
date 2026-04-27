using NJsonSchema;
using NJsonSchema.CodeGeneration;
using NJsonSchema.CodeGeneration.CSharp;

public class ArtifactGenerator
{
    public static readonly string PackageNamespace = "PokeApi.Models";

    public record ArtifactItem(CodeArtifact Artifact, string Filename, string DirectoryPath, string Endpoint);

    public static ArtifactItem[] Generate(JsonSchema[] schemas)
    {
        var settings = new CSharpGeneratorSettings
        {
            Namespace = PackageNamespace,
            ClassStyle = CSharpClassStyle.Poco,
            GenerateDataAnnotations = false,
            GenerateNullableReferenceTypes = true,
            GenerateOptionalPropertiesAsNullable = true,
            GenerateDefaultValues = true,
            HandleReferences = true,
            JsonLibrary = CSharpJsonLibrary.SystemTextJson,
            PropertyNameGenerator = new PascalCasePropertyNameGenerator(),
            TypeNameGenerator = new PascalCaseTypeNameGenerator(),
        };
        // var resolver = new CSharpTypeResolver(settings);

        var allArtifacts = new List<ArtifactItem>();

        foreach (var schema in schemas)
        {
            var csharpGenerator = new CSharpGenerator(schema, settings);
            var artifacts = csharpGenerator.GenerateTypes(schema, "");
            allArtifacts.AddRange(artifacts.Select(a => new ArtifactItem(
                Artifact: a,
                Filename: "",
                DirectoryPath: GetArtifactDirectory(schema.DocumentPath ?? ""),
                Endpoint: GetEndpoint(schema.DocumentPath ?? "") ?? ""
            )));
        }

        // Dedupe by TypeName, handling case conflicts
        var uniqueArtifacts = allArtifacts
            // .GroupBy(item => GetComparableCode(item.Artifact.Code))
            .GroupBy(item => item.Artifact.TypeName)
            .Select(g =>
            {
                var first = g.First();

                foreach (var item in g)
                {
                    if (item.Artifact.Code != first.Artifact.Code)
                    {
                        Console.Error.WriteLine($"Code diff for TypeName={first.Artifact.TypeName}\n{string.Join('\n', g.Select(a => a.Artifact.Code))}");
                        break;
                    }
                }

                return first;
            })

            // handle case conflicts
            .GroupBy(item => item.Artifact.TypeName.ToLower())
            .SelectMany(g =>
            {
                string GetFilename(CodeArtifact a, int i) => g.Count() > 1 ? $"{a.TypeName}{i}" : a.TypeName;

                return g.Select((item, i) => new ArtifactItem(
                    item.Artifact,
                    Filename: GetFilename(item.Artifact, i),
                    item.DirectoryPath,
                    item.Endpoint
                ));
            })
            .OrderBy(v => v.Filename)
            .ToList();

        Console.WriteLine($"{uniqueArtifacts.Count} unique types ({allArtifacts.Count} defore dedupe)");
        Console.WriteLine();

        return [.. uniqueArtifacts];
    }

    public static string GetComparableCode(string raw)
    {
        return string.Join('\n', raw.Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.Length > 0 && !line.StartsWith("//"))
        );
    }

    private static string GetArtifactDirectory(string schemapath)
    {
        var pathParts = schemapath
            .Split(['/', '\\'])
            .Where(s => s.Trim().Length > 0)
            .ToArray();

        if (pathParts.Last().Contains('.'))
        {
            pathParts = pathParts.SkipLast(1).ToArray();
        }

        if (pathParts.Length >= 2 && pathParts[0] == "pokeapi-data" && pathParts[1] == "data")
        {
            pathParts = [.. pathParts.Skip(2)];
        }

        if (pathParts.Length >= 1 && pathParts[0] == "schema")
        {
            pathParts = [.. pathParts.Skip(1)];
        }

        return string.Join('/', pathParts);
    }

    public static string? GetEndpoint(string schemaPath)
    {
        var parts = schemaPath
            .Split(['/', '\\'])
            .Where(s => s.Trim().Length > 0)
            .ToArray();

        var v2Index = -1;
        for (var i = 0; i < parts.Length - 1; i++)
        {
            if (parts[i] == "schema")
            {
                v2Index = i + 1;
                break;
            }
        }

        if (v2Index < 0 || v2Index >= parts.Length)
            return null;

        var segments = parts[v2Index..];

        if (segments.Length == 0)
            return null;

        return "/api/" + string.Join("/", segments);
    }
}
