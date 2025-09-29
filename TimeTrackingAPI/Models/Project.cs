using System.ComponentModel.DataAnnotations;

namespace TimeTrackingAPI.Models
{
    /// <summary>
    /// Модель проекта компании
    /// </summary>
    public class Project
    {
        /// <summary>
        /// Уникальный идентификатор проекта
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название проекта
        /// </summary>
        [Required(ErrorMessage = "Название проекта обязательно")]
        [StringLength(200, ErrorMessage = "Название проекта не должно превышать 200 символов")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Код проекта
        /// </summary>
        [Required(ErrorMessage = "Код проекта обязателен")]
        [StringLength(50, ErrorMessage = "Код проекта не должен превышать 50 символов")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Статус активности проекта
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Коллекция задач, относящихся к данному проекту
        /// </summary>
        public virtual ICollection<WorkTask> Tasks { get; set; } = new List<WorkTask>();
    }
}