using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TimeTrackingAPI.Swagger;

/// <summary>
///     Фильтр для добавления примеров ответов Swagger для TasksController
/// </summary>
public class
        // ReSharper disable once ClassNeverInstantiated.Global
        TasksControllerSwaggerExamplesFilter :
        BaseSwaggerExamplesFilter
{
    private readonly static string[] stringArray =
    [
        "Задача с названием 'Разработка пользовательского интерфейса' уже существует в проекте"
    ];

    private readonly static string[] stringArray0 =
    [
        "Задача с названием 'Разработка пользовательского интерфейса' уже существует в проекте"
    ];

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

        if (controllerName != "TasksController")
            return;

        var actionName = context.MethodInfo.Name;

        switch (actionName)
        {
            case "GetTasks":
                ApplyGetTasksExamples(operation);
                break;
            case "GetActiveTasks":
                ApplyGetActiveTasksExamples(operation);
                break;
            case "GetTask":
                ApplyGetTaskExamples(operation);
                break;
            case "CreateTask":
                ApplyCreateTaskExamples(operation);
                break;
            case "UpdateTask":
                ApplyUpdateTaskExamples(operation);
                break;
            case "DeleteTask":
                ApplyDeleteTaskExamples(operation);
                break;
        }
    }

    private static void ApplyGetTasksExamples(
            OpenApiOperation operation)
    {
        SetJsonExample(operation, "200", new
        {
            data = (object?) new[]
            {
                new
                {
                    id = 1,
                    name =
                            "Разработка пользовательского интерфейса",
                    projectId = 1,
                    isActive = true,
                    project = new
                    {
                        id = 1,
                        name =
                                "Разработка мобильного приложения",
                        code = "MOBILE_APP_2025",
                        isActive = true
                    }
                },
                new
                {
                    id = 2,
                    name = "Тестирование компонентов",
                    projectId = 1,
                    isActive = false,
                    project = new
                    {
                        id = 1,
                        name =
                                "Разработка мобильного приложения",
                        code = "MOBILE_APP_2025",
                        isActive = true
                    }
                }
            },
            statusCode = 200,
            message = "Получено 2 задач(и)",
            isSuccess = true
        });

        SetJsonExample(operation, "500", new
        {
            data = (object?) null,
            statusCode = 500,
            message =
                    "Ошибка при получении задач из базы данных",
            isSuccess = false
        });
    }

    private static void ApplyGetActiveTasksExamples(
            OpenApiOperation operation)
    {
        SetJsonExample(operation, "200", new
        {
            data = (object?) new[]
            {
                new
                {
                    id = 1,
                    name =
                            "Разработка пользовательского интерфейса",
                    projectId = 1,
                    isActive = true,
                    project = new
                    {
                        id = 1,
                        name =
                                "Разработка мобильного приложения",
                        code = "MOBILE_APP_2025",
                        isActive = true
                    }
                }
            },
            statusCode = 200,
            message = "Получено 1 активных задач(и)",
            isSuccess = true
        });

        SetJsonExample(operation, "500", new
        {
            data = (object?) null,
            statusCode = 500,
            message =
                    "Ошибка при получении активных задач из базы данных",
            isSuccess = false
        });
    }

    private static void ApplyGetTaskExamples(
            OpenApiOperation operation)
    {
        SetJsonExample(operation, "200", new
        {
            data = new
            {
                id = 1,
                name = "Разработка пользовательского интерфейса",
                projectId = 1,
                isActive = true,
                project = new
                {
                    id = 1,
                    name = "Разработка мобильного приложения",
                    code = "MOBILE_APP_2025",
                    isActive = true
                }
            },
            statusCode = 200,
            message = "Задача найдена",
            isSuccess = true
        });

        SetJsonExample(operation, "404", new
        {
            data = (object?) null,
            statusCode = 404,
            message = "Задача с ID 999 не найдена",
            isSuccess = false
        });

        SetJsonExample(operation, "500", new
        {
            data = (object?) null,
            statusCode = 500,
            message =
                    "Ошибка при получении задачи из базы данных",
            isSuccess = false
        });
    }

    private static void ApplyCreateTaskExamples(
            OpenApiOperation operation)
    {
        SetJsonExample(operation, "201", new
        {
            data = new
            {
                id = 3,
                name = "Новая задача",
                projectId = 1,
                isActive = true,
                project = new
                {
                    id = 1,
                    name = "Разработка мобильного приложения",
                    code = "MOBILE_APP_2025",
                    isActive = true
                }
            },
            statusCode = 201,
            message = "Задача 'Новая задача' успешно создана",
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
                Name = stringArray
            }
        });

        SetJsonExample(operation, "500", new
        {
            data = (object?) null,
            statusCode = 500,
            message = "Ошибка при создании задачи",
            isSuccess = false
        });
    }

    private static void ApplyUpdateTaskExamples(
            OpenApiOperation operation)
    {
        SetJsonExample(operation, "200", new
        {
            data = new
            {
                id = 1,
                name = "Обновлённое название задачи",
                projectId = 1,
                isActive = false,
                project = new
                {
                    id = 1,
                    name = "Разработка мобильного приложения",
                    code = "MOBILE_APP_2025",
                    isActive = true
                }
            },
            statusCode = 200,
            message = "Задача успешно обновлена",
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
                Name = stringArray0
            }
        });

        SetJsonExample(operation, "404", new
        {
            data = (object?) null,
            statusCode = 404,
            message = "Задача с ID 999 не найдена",
            isSuccess = false
        });

        SetJsonExample(operation, "500", new
        {
            data = (object?) null,
            statusCode = 500,
            message = "Ошибка при обновлении задачи",
            isSuccess = false
        });
    }

    private static void ApplyDeleteTaskExamples(
            OpenApiOperation operation)
    {
        SetJsonExample(operation, "200", new
        {
            data = true,
            statusCode = 200,
            message = "Задача успешно удалена",
            isSuccess = true
        });

        SetJsonExample(operation, "400", new
        {
            data = false,
            statusCode = 400,
            message =
                    "Невозможно удалить задачу. Существуют связанные проводки времени",
            isSuccess = false
        });

        SetJsonExample(operation, "404", new
        {
            data = false,
            statusCode = 404,
            message = "Задача с ID 999 не найдена",
            isSuccess = false
        });

        SetJsonExample(operation, "500", new
        {
            data = false,
            statusCode = 500,
            message = "Ошибка при удалении задачи",
            isSuccess = false
        });
    }
}