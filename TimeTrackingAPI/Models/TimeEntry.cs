using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeTrackingAPI.Models;

/// <summary>
/// Модель проводки рабочего времени
/// </summary>
public sealed class TimeEntry
{
    /// <summary>
    /// Уникальный идентификатор проводки
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Дата проводки (без времени)
    /// </summary>
    [Required(ErrorMessage = "Дата проводки обязательна")]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }

    /// <summary>
    /// Количество часов, потраченных на задачу
    /// </summary>
    [Required(ErrorMessage = "Количество часов обязательно")]
    [Range(0.1, 24.0, ErrorMessage = "Количество часов должно быть от 0.1 до 24")]
    [Column(TypeName = "decimal(4,2)")]
    public decimal Hours { get; set; }

    /// <summary>
    /// Описание выполненной работы
    /// </summary>
    [Required(ErrorMessage = "Описание работы обязательно")]
    [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор задачи, на которую списывается время
    /// </summary>
    [Required(ErrorMessage = "Задача для проводки обязательна")]
    public int TaskId { get; set; }

    /// <summary>
    /// Навигационное свойство к задаче
    /// </summary>
    [ForeignKey(nameof(TaskId))]
    public required WorkTask Task { get; set; }
}