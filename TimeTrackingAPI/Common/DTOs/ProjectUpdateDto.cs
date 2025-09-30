using System.ComponentModel.DataAnnotations;

namespace TimeTrackingAPI.Common.DTOs;

/// <summary>
/// DTO для обновления проекта
/// </summary>
/// <example>
/// {
///   "name": "Разработка веб-приложения",
///   "code": "WEB_APP_2025",
///   "isActive": false
/// }
/// </example>
public sealed record ProjectUpdateDto
{
    /// <summary>
    /// Название проекта
    /// </summary>
    /// <example>Разработка веб-приложения</example>
    [Required(ErrorMessage = "Название проекта обязательно")]
    [StringLength(200, ErrorMessage = "Название проекта не должно превышать 200 символов")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Уникальный код проекта
    /// </summary>
    /// <example>WEB_APP_2025</example>
    [Required(ErrorMessage = "Код проекта обязателен")]
    [StringLength(50, ErrorMessage = "Код проекта не должен превышать 50 символов")]
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Статус активности проекта
    /// </summary>
    /// <example>false</example>
    public bool IsActive { get; init; }
}