using GraphQL.Types;
using GraphQLParser.AST;

namespace OrchardCore.DynamicFields.GraphQL;

public class JsonObjectGraphType : ScalarGraphType
{
    public JsonObjectGraphType()
    {
        Name = "Json";
        Description = "The `Json` scalar type represents arbitrary JSON object.";
    }

    public override object ParseLiteral(GraphQLValue value)
    {
        switch (value)
        {
            case GraphQLStringValue str:
                return str.Value;
            case GraphQLIntValue intVal:
                return intVal.Value;
            case GraphQLBooleanValue boolVal:
                return boolVal.Value;
            case GraphQLObjectValue objVal:
                var dict = new Dictionary<string, object>();
                foreach (var field in objVal.Fields)
                {
                    dict[field.Name.StringValue] = ParseLiteral(field.Value);
                }
                return dict;
            case GraphQLListValue listVal:
                return listVal.Values.Select(ParseLiteral).ToList();
            default:
                return null;
        }
    }

    public override object ParseValue(object value)
    {
        return value;
    }

    public override object Serialize(object value)
    {
        return value;
    }
}
