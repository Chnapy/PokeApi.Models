using NJsonSchema;

public class PascalCaseTypeNameGenerator : ITypeNameGenerator
{
    private readonly DefaultTypeNameGenerator _default = new();

    public string Generate(JsonSchema schema, string? typeNameHint, IEnumerable<string> reservedTypeNames)
    {
        // Let NJsonSchema compute base name
        var baseName = _default.Generate(schema, typeNameHint, reservedTypeNames);

        return ToPascalCase(baseName);
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
