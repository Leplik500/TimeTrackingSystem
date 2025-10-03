using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TimeTrackingAPI.Swagger;

/// <summary>
///     Фильтр для добавления примеров ответов Swagger для TimeEntriesController
/// </summary>
public class
        TimeEntriesControllerSwaggerExamplesFilter :
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

        if (controllerName != "TimeEntriesController")
            return;

        var actionName = context.MethodInfo.Name;

        switch (actionName)
        {
            case "GetTimeEntries":
                ApplyGetTimeEntriesExamples(operation);
                break;
            case "GetTimeEntriesByDate":
                ApplyGetTimeEntriesByDateExamples(operation);
                break;
            case "GetTimeEntriesByMonth":
                ApplyGetTimeEntriesByMonthExamples(operation);
                break;
            case "CreateTimeEntry":
                ApplyCreateTimeEntryExamples(operation);
                break;
            case "GetDailySummary":
                ApplyGetDailySummaryExamples(operation);
                break;
        }
    }

    private static void ApplyGetTimeEntriesExamples(
            OpenApiOperation operation)
    {
        SetJsonExample(operation, "200", new
        {
            data = (object?) new[]
            {
                new
                {
                    id = 1,
                    date = "2025-10-03",
                    hours = 8.5m,
                    description =
                            "Разработка функциональности регистрации пользователей",
                    taskId = 1,
                    task = new
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
                new
                {
                    id = 2,
                    date = "2025-10-02",
                    hours = 6.0m,
                    description =
                            "Исправление багов в системе авторизации",
                    taskId = 1,
                    task = new
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
                }
            },
            statusCode = 200,
            message = "Получено 2 проводок времени",
            isSuccess = true
        });

        SetJsonExample(operation, "500", new
        {
            data = (object?) null,
            statusCode = 500,
            message =
                    "Ошибка при получении проводок времени из базы данных",
            isSuccess = false
        });
    }

    private static void ApplyGetTimeEntriesByDateExamples(
            OpenApiOperation operation)
    {
        SetJsonExample(operation, "200", new
        {
            data = (object?) new[]
            {
                new
                {
                    id = 1,
                    date = "2025-10-03",
                    hours = 8.5m,
                    description =
                            "Разработка функциональности регистрации пользователей",
                    taskId = 1,
                    task = new
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
                }
            },
            statusCode = 200,
            message =
                    "Получено 1 проводок времени за дату 2025-10-03",
            isSuccess = true
        });

        SetJsonExample(operation, "500", new
        {
            data = (object?) null,
            statusCode = 500,
            message =
                    "Ошибка при получении проводок времени за дату",
            isSuccess = false
        });
    }

    private static void ApplyGetTimeEntriesByMonthExamples(
            OpenApiOperation operation)
    {
        SetJsonExample(operation, "200", new
        {
            data = (object?) new[]
            {
                new
                {
                    id = 1,
                    date = "2025-10-03",
                    hours = 8.5m,
                    description =
                            "Разработка функциональности регистрации пользователей",
                    taskId = 1,
                    task = new
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
                new
                {
                    id = 2,
                    date = "2025-10-15",
                    hours = 7.0m,
                    description =
                            "Тестирование системы уведомлений",
                    taskId = 1,
                    task = new
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
                }
            },
            statusCode = 200,
            message =
                    "Получено 2 проводок времени за октябрь 2025",
            isSuccess = true
        });

        SetJsonExample(operation, "400", new
        {
            data = (object?) null,
            statusCode = 400,
            message = "Месяц должен быть от 1 до 12",
            isSuccess = false
        });

        SetJsonExample(operation, "500", new
        {
            data = (object?) null,
            statusCode = 500,
            message =
                    "Ошибка при получении проводок времени за месяц",
            isSuccess = false
        });
    }

    private static void ApplyCreateTimeEntryExamples(
            OpenApiOperation operation)
    {
        SetJsonExample(operation, "201", new
        {
            data = (object?) new
            {
                id = 3,
                date = "2025-10-03",
                hours = 8.0m,
                description =
                        "Разработка новой функциональности",
                taskId = 1,
                task = new
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
            statusCode = 201,
            message = "Проводка времени успешно создана",
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
                Hours = new[]
                {
                    "Сумма часов за день 2025-10-03 не может превышать 24 часа. Текущая сумма: 16, добавляется: 9, итого: 25"
                }
            }
        });

        SetJsonExample(operation, "500", new
        {
            data = (object?) null,
            statusCode = 500,
            message = "Ошибка при создании проводки времени",
            isSuccess = false
        });
    }

    private static void ApplyGetDailySummaryExamples(
            OpenApiOperation operation)
    {
        SetJsonExample(operation, "200", new
        {
            data = (object?) new[]
            {
                new
                {
                    date = "2025-10-03",
                    totalHours = 8.5m,
                    status = "excessive"
                },
                new
                {
                    date = "2025-10-02",
                    totalHours = 8.0m,
                    status = "sufficient"
                },
                new
                {
                    date = "2025-10-01",
                    totalHours = 6.5m,
                    status = "insufficient"
                }
            },
            statusCode = 200,
            message = "Получено 3 дневных сводок",
            isSuccess = true
        });

        SetJsonExample(operation, "500", new
        {
            data = (object?) null,
            statusCode = 500,
            message = "Ошибка при получении дневной сводки",
            isSuccess = false
        });
    }
}