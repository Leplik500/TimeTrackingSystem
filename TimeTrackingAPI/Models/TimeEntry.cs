using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeTrackingAPI.Models;

/// <summary>
///     Модель проводки рабочего времени
/// </summary>
/// <remarks>
///     Проводка представляет собой запись времени, затраченного на выполнение
///     задачи в конкретный день.
///     Включает валидацию: сумма часов за день не может превышать 24 часа.
/// </remarks>
/// <example>
///     {
///     "id": 1,
///     "date": "2025-01-15",
///     "hours": 4.5,
///     "description": "Разработка компонентов пользовательского интерфейса",
///     "taskId": 1
///     }
/// </example>
public sealed class TimeEntry
{
    /// <summary>
    ///     Уникальный идентификатор проводки
    /// </summary>
    /// <value>Автоматически генерируется при создании записи в базе данных</value>
    /// <example>1</example>
    public int Id { get; init; }

    /// <summary>
    ///     Дата выполнения работы
    /// </summary>
    /// <value>Дата без компонента времени</value>
    /// <remarks>При создании проводки компонент времени автоматически обнуляется</remarks>
    /// <example>2025-01-15</example>
    [Required(ErrorMessage = "Дата проводки обязательна")]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }

    /// <summary>
    ///     Количество часов, потраченных на задачу
    /// </summary>
    /// <value>Десятичное число от 0.1 до 24 с точностью до двух знаков после запятой</value>
    /// <remarks>
    ///     Валидируется при создании: сумма всех часов за день не должна
    ///     превышать 24
    /// </remarks>
    /// <example>4.5</example>
    [Required(ErrorMessage = "Количество часов обязательно")]
    [Range(0.1, 24.0,
            ErrorMessage =
                    "Количество часов должно быть от 0.1 до 24")]
    [Column(TypeName = "decimal(4,2)")]
    public decimal Hours { get; set; }

    /// <summary>
    ///     Описание выполненной работы
    /// </summary>
    /// <value>Обязательное поле, максимальная длина 500 символов</value>
    /// <example>Разработка компонентов пользовательского интерфейса</example>
    [Required(ErrorMessage = "Описание работы обязательно")]
    [StringLength(500,
            ErrorMessage =
                    "Описание не должно превышать 500 символов")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     Идентификатор задачи, на которую списывается время
    /// </summary>
    /// <value>Внешний ключ на таблицу WorkTasks</value>
    /// <example>1</example>
    [Required(ErrorMessage = "Задача для проводки обязательна")]
    public int TaskId { get; set; }

    /// <summary>
    ///     Навигационное свойство к задаче, на которую списывается время
    /// </summary>
    /// <value>Ссылка на связанный объект WorkTask</value>
    /// <remarks>Загружается через Include() при выполнении запросов Entity Framework</remarks>
    [ForeignKey(nameof(TaskId))]
    public required WorkTask Task { get; init; }
}