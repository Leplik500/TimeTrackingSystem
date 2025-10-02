using System.ComponentModel.DataAnnotations;

namespace TimeTrackingAPI.Models;

/// <summary>
///     Модель проекта компании
/// </summary>
/// <remarks>
///     Проект представляет собой основную единицу для группировки задач и учета
///     времени.
///     Каждый проект имеет уникальный код и может содержать множество рабочих
///     задач.
/// </remarks>
/// <example>
///     {
///     "id": 1,
///     "name": "Разработка мобильного приложения",
///     "code": "MOBILE_APP_2025",
///     "isActive": true
///     }
/// </example>
public sealed class Project
{
    /// <summary>
    ///     Уникальный идентификатор проекта
    /// </summary>
    /// <value>Автоматически генерируется при создании записи в базе данных</value>
    /// <example>1</example>
    public int Id { get; init; }

    /// <summary>
    ///     Название проекта
    /// </summary>
    /// <value>Обязательное поле, максимальная длина 200 символов</value>
    /// <example>Разработка мобильного приложения</example>
    [Required(ErrorMessage = "Название проекта обязательно")]
    [StringLength(200,
            ErrorMessage =
                    "Название проекта не должно превышать 200 символов")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Уникальный код проекта
    /// </summary>
    /// <value>Обязательное поле, максимальная длина 50 символов</value>
    /// <remarks>
    ///     Используется для краткой идентификации проекта. Должен быть уникальным
    ///     в системе
    /// </remarks>
    /// <example>MOBILE_APP_2025</example>
    [Required(ErrorMessage = "Код проекта обязателен")]
    [StringLength(50,
            ErrorMessage =
                    "Код проекта не должен превышать 50 символов")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    ///     Статус активности проекта
    /// </summary>
    /// <value>true - проект активен, false - проект неактивен. По умолчанию true</value>
    /// <remarks>Неактивные проекты могут быть скрыты в интерфейсе пользователя</remarks>
    /// <example>true</example>
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     Коллекция задач, относящихся к данному проекту
    /// </summary>
    /// <value>Навигационное свойство для связи один-ко-многим с рабочими задачами</value>
    /// <remarks>Используется Entity Framework для ленивой загрузки связанных задач</remarks>
    public ICollection<WorkTask> Tasks { get; init; } =
        new List<WorkTask>();
}