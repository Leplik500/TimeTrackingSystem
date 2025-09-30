using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeTrackingAPI.Models
{
    /// <summary>
    /// Модель рабочей задачи
    /// </summary>
    public sealed class WorkTask
    {
        /// <summary>
        /// Уникальный идентификатор задачи
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название задачи
        /// </summary>
        [Required(ErrorMessage = "Название задачи обязательно")]
        [StringLength(300, ErrorMessage = "Название задачи не должно превышать 300 символов")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Идентификатор проекта, к которому относится задача
        /// </summary>
        [Required(ErrorMessage = "Проект для задачи обязателен")]
        public int ProjectId { get; set; }

        /// <summary>
        /// Статус активности задачи
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Навигационное свойство к проекту
        /// </summary>
        [ForeignKey(nameof(ProjectId))]
        public required Project Project { get; set; }

        /// <summary>
        /// Коллекция проводок времени для данной задачи
        /// </summary>
        public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    }
}