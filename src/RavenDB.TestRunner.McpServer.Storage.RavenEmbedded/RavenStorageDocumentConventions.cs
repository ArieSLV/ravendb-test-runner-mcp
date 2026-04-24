using System.Text.Json;
using Newtonsoft.Json.Serialization;
using Raven.Client.Documents.Conventions;
using Raven.Client.Json.Serialization.NewtonsoftJson;

namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public static class RavenStorageDocumentConventions
{
    public static DocumentConventions Create()
    {
        return new DocumentConventions
        {
            UseOptimisticConcurrency = true,
            Serialization = new NewtonsoftJsonSerializationConventions
            {
                CustomizeJsonSerializer = serializer => serializer.ContractResolver = new CamelCasePropertyNamesContractResolver(),
                CustomizeJsonDeserializer = serializer => serializer.ContractResolver = new CamelCasePropertyNamesContractResolver()
            },
            PropertyNameConverter = ToCamelCase,
#pragma warning disable CS0618
            // RavenDB 7.x requires this temporary hook for LINQ-built index definitions to honor property naming.
            FindPropertyNameForIndexDefinition = ToCamelCase
#pragma warning restore CS0618
        };
    }

    private static string ToCamelCase(System.Reflection.MemberInfo member)
    {
        return JsonNamingPolicy.CamelCase.ConvertName(member.Name);
    }
}
