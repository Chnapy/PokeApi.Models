using NJsonSchema;

Console.WriteLine("- Schema Generator -");

var input = args[0];
var output = args[1];
var changeResultFile = args[2];

Console.WriteLine($"Input : {input}");
Console.WriteLine($"Output : {output}");
Console.WriteLine($"Change result file : {changeResultFile}");
Console.WriteLine();

var schemaRoot = new DirectoryInfo(input);

var schemaFiles = Directory.GetFiles(input, "*.json", SearchOption.AllDirectories)
    .Order()
    .ToList();

if (schemaFiles.Count == 0)
{
    throw new ArgumentException("No JSON file found");
}

Console.WriteLine($"{schemaFiles.Count} schemas found");

var schemas = new List<JsonSchema>();

foreach (var schemaFile in schemaFiles)
{
    var schema = await JsonSchema.FromFileAsync(
        schemaFile,
        rootSchema => new PokeApiReferenceResolver(rootSchema, schemaRoot.FullName));
    schemas.Add(schema);
}

var uniqueArtifacts = ArtifactGenerator.Generate([.. schemas]);

var fileManager = new FileManager(output, changeResultFile);

// fileManager.DebugDeleteAll();

await fileManager.WriteFiles(uniqueArtifacts);
