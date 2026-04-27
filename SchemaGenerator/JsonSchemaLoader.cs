using Newtonsoft.Json.Linq;
using NJsonSchema;

public class JsonSchemaLoader(string input)
{
    public async Task<JsonSchema[]> Load(string[] schemaFilePaths)
    {
        var schemaRoot = new DirectoryInfo(input);

        var schemas = new List<JsonSchema>();

        foreach (var schemaFile in schemaFilePaths)
        {
            var fileContent = await File.ReadAllTextAsync(schemaFile);

            var baseSchema = JToken.Parse(fileContent);

            NormalizeBaseSchema(baseSchema);

            var schema = await JsonSchema.FromJsonAsync(
                baseSchema.ToString(),
                schemaFile,
                rootSchema => new PokeApiReferenceResolver(rootSchema, schemaRoot.FullName));
            schemas.Add(schema);
        }

        return [.. schemas];
    }

    /**
     * - Remove { type: null } from anyOf/oneOf, and remove property from 'required'.
     */
    private void NormalizeBaseSchema(JToken schema)
    {
        switch (schema.Type)
        {
            case JTokenType.Object:

                var schemaObj = (JObject)schema;
                var schemaProperties = schemaObj.Properties();

                var propProperties = schemaProperties.FirstOrDefault(prop => prop.Name == "properties")?.Value;
                var propRequired = schemaProperties.FirstOrDefault(prop => prop.Name == "required")?.Value;

                if (propProperties?.Type == JTokenType.Object && propRequired?.Type == JTokenType.Array)
                {
                    var propertiesObj = (JObject)propProperties;
                    var requiredObj = (JArray)propRequired;

                    foreach (var property in propertiesObj.Properties())
                    {
                        if (property.Value.Type != JTokenType.Object)
                            continue;

                        foreach (var prop in ((JObject)property.Value).Properties())
                        {

                            if (prop.Name == "anyOf" || prop.Name == "oneOf")
                            {
                                var values = prop.Value.Children();

                                var nullTypes = values.Where(value => ((JObject)value).Properties()
                                    .Any(obj => obj.Name == "type" && obj.Value.ToString() == "null")
                                );

                                if (nullTypes.Any())
                                {
                                    foreach (var type in nullTypes.ToList())
                                        type.Remove();

                                    var requiredToRemove = requiredObj.Where(t =>
                                        t.Type == JTokenType.String
                                        && t.Value<string>() == property.Name
                                    ).ToArray();

                                    foreach (var req in requiredToRemove)
                                        req.Remove();
                                }
                            }
                        }
                    }
                }

                foreach (var prop in ((JObject)schema).Properties())
                {
                    NormalizeBaseSchema(prop.Value);
                }
                break;
            case JTokenType.Array:
                foreach (var item in schema.ToArray())
                {
                    NormalizeBaseSchema(item);
                }
                break;
        }
    }
}
