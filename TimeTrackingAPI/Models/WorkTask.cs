using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeTrackingAPI.Models;

/// <summary>
///     Модель рабочей задачи в рамках проекта
/// </summary>
/// <remarks>
///     Задача представляет собой конкретную работу в рамках проекта.
///     Для задачи могут создаваться проводки времени только при её активном
///     статусе.
/// </remarks>
/// <example>
///     {
///     "id": 1,
///     "name": "Разработка пользовательского интерфейса",
///     "projectId": 1,
///     "isActive": true
///     }
/// </example>
public sealed class WorkTask
{
    /// <summary>
    ///     Уникальный идентификатор задачи
    /// </summary>
    /// <value>Автоматически генерируется при создании записи в базе данных</value>
    /// <example>1</example>
    public int Id { get; init; }

    /// <summary>
    ///     Название задачи
    /// </summary>
    /// <value>Обязательное поле, максимальная длина 300 символов</value>
    /// <example>Разработка пользовательского интерфейса</example>
    [Required(ErrorMessage = "Название задачи обязательно")]
    [StringLength(300,
            ErrorMessage =
                    "Название задачи не должно превышать 300 символов")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Идентификатор проекта, к которому относится задача
    /// </summary>
    /// <value>Внешний ключ на таблицу Projects</value>
    /// <example>1</example>
    [Required(ErrorMessage = "Проект для задачи обязателен")]
    public int ProjectId { get; set; }

    /// <summary>
    ///     Статус активности задачи
    /// </summary>
    /// <value>true - задача активна, false - задача неактивна. По умолчанию true</value>
    /// <remarks>Проводки времени можно создавать только для активных задач</remarks>
    /// <example>true</example>
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     Навигационное свойство к проекту, которому принадлежит задача
    /// </summary>
    /// <value>Ссылка на связанный объект Project</value>
    /// <remarks>Загружается через Include() при выполнении запросов Entity Framework</remarks>
    [ForeignKey(nameof(ProjectId))]
    public required Project Project { get; init; }

    /// <summary>
    ///     Коллекция проводок времени для данной задачи
    /// </summary>
    /// <value>Навигационное свойство для связи один-ко-многим с проводками времени</value>
    /// <remarks>Используется для проверки возможности удаления задачи</remarks>
    public ICollection<TimeEntry> TimeEntries { get; init; } =
        new List<TimeEntry>();
}