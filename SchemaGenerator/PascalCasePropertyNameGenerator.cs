using NJsonSchema;
using NJsonSchema.CodeGeneration;

public class PascalCasePropertyNameGenerator : IPropertyNameGenerator
{
    public string Generate(JsonSchemaProperty property)
    {
        return PascalCaseTypeNameGenerator.ToPascalCase(property.Name);
    }
}
