using System.ComponentModel.DataAnnotations;

namespace TimeTrackingAPI.Common.DTOs;

/// <summary>
///     DTO для обновления существующей рабочей задачи
/// </summary>
/// <remarks>
///     Используется в PUT /api/tasks/{id} для передачи обновленных данных задачи.
///     Перед обновлением проверяется существование указанного проекта.
/// </remarks>
/// <example>
///     {
///     "name": "Тестирование пользовательского интерфейса",
///     "projectId": 2,
///     "isActive": false
///     }
/// </example>
public sealed record TaskUpdateDto
{
    /// <summary>
    ///     Название задачи
    /// </summary>
    /// <example>Тестирование пользовательского интерфейса</example>
    [Required(ErrorMessage = "Название задачи обязательно")]
    [StringLength(300,
            ErrorMessage =
                    "Название задачи не должно превышать 300 символов")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    ///     Идентификатор проекта, к которому относится задача
    /// </summary>
    /// <example>2</example>
    [Required(ErrorMessage = "Проект для задачи обязателен")]
    [Range(1, int.MaxValue,
            ErrorMessage =
                    "Идентификатор проекта должен быть больше 0")]
    public int ProjectId { get; init; }

    /// <summary>
    ///     Статус активности задачи
    /// </summary>
    /// <example>false</example>
    public bool IsActive { get; init; }
}