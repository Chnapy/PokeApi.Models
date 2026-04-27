using NJsonSchema;

Console.WriteLine("- Schema Generator -");

var input = args[0];
var output = args[1];
var changeResultFile = args[2];

Console.WriteLine($"Input : {input}");
Console.WriteLine($"Output : {output}");
Console.WriteLine($"Change result file : {changeResultFile}");
Console.WriteLine();

var schemaFiles = Directory.GetFiles(input, "*.json", SearchOption.AllDirectories)
    .Order();

if (!schemaFiles.Any())
{
    throw new ArgumentException("No JSON file found");
}

Console.WriteLine($"{schemaFiles.Count()} schemas found");

var schemaLoader = new JsonSchemaLoader(input);

var schemas = await schemaLoader.Load([.. schemaFiles]);

var uniqueArtifacts = ArtifactGenerator.Generate([.. schemas]);

var fileManager = new FileManager(output, changeResultFile);

// fileManager.DebugDeleteAll();

await fileManager.WriteFiles(uniqueArtifacts);
