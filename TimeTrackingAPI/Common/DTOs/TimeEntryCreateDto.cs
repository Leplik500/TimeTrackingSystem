using System.ComponentModel.DataAnnotations;

namespace TimeTrackingAPI.Common.DTOs;

/// <summary>
///     DTO для создания новой проводки времени
/// </summary>
/// <remarks>
///     Используется в POST /api/timeentries для передачи данных новой проводки.
///     Перед созданием проверяется активность задачи и валидируется сумма часов за
///     день (не более 24).
/// </remarks>
/// <example>
///     {
///     "date": "2025-01-15",
///     "hours": 4.5,
///     "description": "Разработка пользовательского интерфейса",
///     "taskId": 1
///     }
/// </example>
public sealed record TimeEntryCreateDto
{
    /// <summary>
    ///     Дата проводки
    /// </summary>
    /// <example>2025-01-15</example>
    [Required(ErrorMessage = "Дата проводки обязательна")]
    public DateTime Date { get; init; }

    /// <summary>
    ///     Количество часов
    /// </summary>
    /// <example>4.5</example>
    [Required(ErrorMessage = "Количество часов обязательно")]
    [Range(0.1, 24.0,
            ErrorMessage =
                    "Количество часов должно быть от 0.1 до 24")]
    public decimal Hours { get; init; }

    /// <summary>
    ///     Описание выполненной работы
    /// </summary>
    /// <example>Разработка пользовательского интерфейса</example>
    [Required(ErrorMessage = "Описание работы обязательно")]
    [StringLength(500,
            ErrorMessage =
                    "Описание не должно превышать 500 символов")]
    public string Description { get; init; } = string.Empty;

    /// <summary>
    ///     Идентификатор задачи
    /// </summary>
    /// <example>1</example>
    [Required(ErrorMessage = "Задача для проводки обязательна")]
    [Range(1, int.MaxValue,
            ErrorMessage =
                    "Идентификатор задачи должен быть больше 0")]
    public int TaskId { get; init; }
}