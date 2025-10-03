using System.Text.Json;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TimeTrackingAPI.Swagger;

/// <summary>
///     Базовый класс для фильтров примеров ответов Swagger
/// </summary>
public abstract class
        BaseSwaggerExamplesFilter : IOperationFilter
{
    private readonly static JsonSerializerOptions JsonOptions =
            new()
            {
                PropertyNamingPolicy =
                        JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

    /// <summary>
    ///     Применяет примеры ответов к операциям Swagger
    /// </summary>
    /// <param name="operation">Swagger операция</param>
    /// <param name="context">Контекст операции</param>
    public abstract void Apply(OpenApiOperation operation,
            OperationFilterContext context);

    /// <summary>
    ///     Устанавливает JSON пример для указанного кода ответа
    /// </summary>
    /// <param name="operation">OpenAPI операция</param>
    /// <param name="statusCode">Код ответа</param>
    /// <param name="exampleObject">Объект примера</param>
    protected static void SetJsonExample(
            OpenApiOperation operation,
            string statusCode,
            object exampleObject)
    {
        if (!operation.Responses.TryGetValue(statusCode,
                    out var response))
            return;

        foreach (var mediaType in response.Content.Values)
        {
            var jsonString =
                    JsonSerializer.Serialize(exampleObject,
                            JsonOptions);

            var jsonDocument = JsonDocument.Parse(jsonString);
            var openApiAny =
                    ConvertToOpenApiAny(jsonDocument
                            .RootElement);

            mediaType.Example = openApiAny;
        }
    }

    /// <summary>
    ///     Конвертирует JsonElement в OpenApiAny
    /// </summary>
    protected static IOpenApiAny ConvertToOpenApiAny(
            JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => CreateOpenApiObject(element),
            JsonValueKind.Array => CreateOpenApiArray(element),
            JsonValueKind.String => new OpenApiString(
                    element.GetString()),
            JsonValueKind.Number => element.TryGetInt32(
                    out var intValue)
                    ? new OpenApiInteger(intValue)
                    : new OpenApiDouble(element.GetDouble()),
            JsonValueKind.True => new OpenApiBoolean(true),
            JsonValueKind.False => new OpenApiBoolean(false),
            JsonValueKind.Null => new OpenApiNull(),
            _ => new OpenApiString(element.ToString())
        };
    }

    /// <summary>
    ///     Создает OpenApiObject из JsonElement
    /// </summary>
    private static OpenApiObject CreateOpenApiObject(
            JsonElement element)
    {
        var openApiObject = new OpenApiObject();
        foreach (var property in element.EnumerateObject())
        {
            openApiObject[property.Name] =
                    ConvertToOpenApiAny(property.Value);
        }

        return openApiObject;
    }

    /// <summary>
    ///     Создает OpenApiArray из JsonElement
    /// </summary>
    private static OpenApiArray CreateOpenApiArray(
            JsonElement element)
    {
        var openApiArray = new OpenApiArray();
        openApiArray.AddRange(element.EnumerateArray()
                .Select(ConvertToOpenApiAny));

        return openApiArray;
    }
}