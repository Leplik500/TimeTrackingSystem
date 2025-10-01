using System.ComponentModel.DataAnnotations;

namespace TimeTrackingAPI.Common.DTOs
{
    /// <summary>
    /// DTO для создания новой рабочей задачи
    /// </summary>
    /// <example>
    /// {
    ///   "name": "Разработка пользовательского интерфейса",
    ///   "projectId": 1,
    ///   "isActive": true
    /// }
    /// </example>
    public sealed record TaskCreateDto
    {
        /// <summary>
        /// Название задачи
        /// </summary>
        /// <example>Разработка пользовательского интерфейса</example>
        [Required(ErrorMessage = "Название задачи обязательно")]
        [StringLength(300, ErrorMessage = "Название задачи не должно превышать 300 символов")]
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Идентификатор проекта, к которому относится задача
        /// </summary>
        /// <example>1</example>
        [Required(ErrorMessage = "Проект для задачи обязателен")]
        [Range(1, int.MaxValue, ErrorMessage = "Идентификатор проекта должен быть больше 0")]
        public int ProjectId { get; init; }

        /// <summary>
        /// Статус активности задачи (по умолчанию true)
        /// </summary>
        /// <example>true</example>
        public bool IsActive { get; init; } = true;
    }
}