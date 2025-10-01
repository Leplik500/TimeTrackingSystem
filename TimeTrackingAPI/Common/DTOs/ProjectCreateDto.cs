using System.ComponentModel.DataAnnotations;

namespace TimeTrackingAPI.Common.DTOs;

/// <summary>
///     DTO для создания нового проекта
/// </summary>
/// <example>
///     {
///     "name": "Разработка мобильного приложения",
///     "code": "MOBILE_APP_2025",
///     "isActive": true
///     }
/// </example>
public sealed record ProjectCreateDto
{
    /// <summary>
    ///     Название проекта
    /// </summary>
    /// <example>Разработка мобильного приложения</example>
    [Required(ErrorMessage = "Название проекта обязательно")]
    [StringLength(200,
            ErrorMessage =
                    "Название проекта не должно превышать 200 символов")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    ///     Уникальный код проекта
    /// </summary>
    /// <example>MOBILE_APP_2025</example>
    [Required(ErrorMessage = "Код проекта обязателен")]
    [StringLength(50,
            ErrorMessage =
                    "Код проекта не должен превышать 50 символов")]
    public string Code { get; init; } = string.Empty;

    /// <summary>
    ///     Статус активности проекта (по умолчанию true)
    /// </summary>
    /// <example>true</example>
    public bool IsActive { get; init; } = true;
}