using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TimeTrackingAPI.Swagger;

/// <summary>
///     Фильтр для добавления примеров ответов Swagger для ProjectsController
/// </summary>
public class
        ProjectsControllerSwaggerExamplesFilter :
        BaseSwaggerExamplesFilter
{
    /// <summary>
    ///     Применяет примеры ответов к операциям Swagger
    /// </summary>
    /// <param name="operation">Swagger операция</param>
    /// <param name="context">Контекст операции</param>
    public override void Apply(OpenApiOperation operation,
            OperationFilterContext context)
    {
        var controllerName =
                context.MethodInfo.DeclaringType?.Name;

        if (controllerName != "ProjectsController")
            return;

        var actionName = context.MethodInfo.Name;

        switch (actionName)
        {
            case "GetProjects":
                ApplyGetProjectsExamples(operation);
                break;
            case "GetProject":
                ApplyGetProjectExamples(operation);
                break;
            case "CreateProject":
                ApplyCreateProjectExamples(operation);
                break;
            case "UpdateProject":
                ApplyUpdateProjectExamples(operation);
                break;
            case "DeleteProject":
                ApplyDeleteProjectExamples(operation);
                break;
        }
    }

    private static void ApplyGetProjectsExamples(
            OpenApiOperation operation)
    {
        SetJsonExample(operation, "200", new
        {
            data = (object?) new[]
            {
                new
                {
                    id = 1,
                    name = "Разработка мобильного приложения",
                    code = "MOBILE_APP_2025",
                    isActive = true
                },
                new
                {
                    id = 2,
                    name = "Веб-портал компании",
                    code = "WEB_PORTAL_2025",
                    isActive = true
                },
                new
                {
                    id = 3,
                    name = "Система управления складом",
                    code = "WMS_2024",
                    isActive = false
                }
            },
            statusCode = 200,
            message = "Получено 3 проектов",
            isSuccess = true
        });

        SetJsonExample(operation, "500", new
        {
            data = (object?) null,
            statusCode = 500,
            message =
                    "Ошибка при получении проектов из базы данных",
            isSuccess = false
        });
    }

    private static void ApplyGetProjectExamples(
            OpenApiOperation operation)
    {
        SetJsonExample(operation, "200", new
        {
            data = (object?) new
            {
                id = 1,
                name = "Разработка мобильного приложения",
                code = "MOBILE_APP_2025",
                isActive = true
            },
            statusCode = 200,
            message = "Проект найден",
            isSuccess = true
        });

        SetJsonExample(operation, "404", new
        {
            data = (object?) null,
            statusCode = 404,
            message = "Проект с ID 999 не найден",
            isSuccess = false
        });

        SetJsonExample(operation, "500", new
        {
            data = (object?) null,
            statusCode = 500,
            message =
                    "Ошибка при получении проекта из базы данных",
            isSuccess = false
        });
    }

    private static void ApplyCreateProjectExamples(
            OpenApiOperation operation)
    {
        SetJsonExample(operation, "201", new
        {
            data = (object?) new
            {
                id = 4,
                name = "Новый проект",
                code = "NEW_PROJECT_2025",
                isActive = true
            },
            statusCode = 201,
            message = "Проект 'Новый проект' успешно создан",
            isSuccess = true
        });

        SetJsonExample(operation, "400", new
        {
            type =
                    "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            title = "One or more validation errors occurred.",
            status = 400,
            errors = new
            {
                Code = new[]
                {
                    "Проект с кодом 'MOBILE_APP_2025' уже существует"
                }
            }
        });

        SetJsonExample(operation, "500", new
        {
            data = (object?) null,
            statusCode = 500,
            message = "Ошибка при создании проекта",
            isSuccess = false
        });
    }

    private static void ApplyUpdateProjectExamples(
            OpenApiOperation operation)
    {
        SetJsonExample(operation, "200", new
        {
            data = (object?) new
            {
                id = 1,
                name = "Обновлённое название проекта",
                code = "UPDATED_PROJECT_2025",
                isActive = false
            },
            statusCode = 200,
            message = "Проект успешно обновлен",
            isSuccess = true
        });

        SetJsonExample(operation, "400", new
        {
            type =
                    "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            title = "One or more validation errors occurred.",
            status = 400,
            errors = new
            {
                Code = new[]
                {
                    "Проект с кодом 'WEB_PORTAL_2025' уже существует"
                }
            }
        });

        SetJsonExample(operation, "404", new
        {
            data = (object?) null,
            statusCode = 404,
            message = "Проект с ID 999 не найден",
            isSuccess = false
        });

        SetJsonExample(operation, "500", new
        {
            data = (object?) null,
            statusCode = 500,
            message = "Ошибка при обновлении проекта",
            isSuccess = false
        });
    }

    private static void ApplyDeleteProjectExamples(
            OpenApiOperation operation)
    {
        SetJsonExample(operation, "200", new
        {
            data = true,
            statusCode = 200,
            message = "Проект успешно удален",
            isSuccess = true
        });

        SetJsonExample(operation, "400", new
        {
            data = false,
            statusCode = 400,
            message =
                    "Невозможно удалить проект. Существуют связанные задачи",
            isSuccess = false
        });

        SetJsonExample(operation, "404", new
        {
            data = false,
            statusCode = 404,
            message = "Проект с ID 999 не найден",
            isSuccess = false
        });

        SetJsonExample(operation, "500", new
        {
            data = false,
            statusCode = 500,
            message = "Ошибка при удалении проекта",
            isSuccess = false
        });
    }
}