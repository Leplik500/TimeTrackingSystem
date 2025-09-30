using System.ComponentModel.DataAnnotations;

namespace TimeTrackingAPI.Models;

/// <summary>
/// Модель проекта компании
/// </summary>
/// <remarks>
/// Проект представляет собой основную единицу для группировки задач и учета времени.
/// Каждый проект имеет уникальный код и может содержать множество рабочих задач.
/// </remarks>
/// <example>
/// {
///   "id": 1,
///   "name": "Разработка мобильного приложения",
///   "code": "MOBILE_APP_2025",
///   "isActive": true
/// }
/// </example>
public sealed class Project
{
    /// <summary>
    /// Уникальный идентификатор проекта
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// Название проекта
    /// </summary>
    /// <example>Разработка мобильного приложения</example>
    [Required(ErrorMessage = "Название проекта обязательно")]
    [StringLength(200, ErrorMessage = "Название проекта не должно превышать 200 символов")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Уникальный код проекта
    /// </summary>
    /// <example>MOBILE_APP_2025</example>
    [Required(ErrorMessage = "Код проекта обязателен")]
    [StringLength(50, ErrorMessage = "Код проекта не должен превышать 50 символов")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Статус активности проекта (true - активен, false - неактивен)
    /// </summary>
    /// <example>true</example>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Коллекция задач, относящихся к данному проекту
    /// </summary>
    public ICollection<WorkTask> Tasks { get; set; } = new List<WorkTask>();
}