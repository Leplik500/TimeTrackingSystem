using TimeTrackingAPI.Common.Models;
using TimeTrackingAPI.Models;

namespace TimeTrackingAPI.Services.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса для работы с рабочими задачами
    /// </summary>
    public interface ITaskService
    {
        /// <summary>
        /// Получить все рабочие задачи
        /// </summary>
        /// <returns>Список всех рабочих задач</returns>
        Task<ApiResponse<List<WorkTask>>> GetAllTasksAsync();

        /// <summary>
        /// Получить только активные рабочие задачи
        /// </summary>
        /// <returns>Список активных рабочих задач</returns>
        Task<ApiResponse<List<WorkTask>>> GetActiveTasksAsync();

        /// <summary>
        /// Получить рабочую задачу по ID
        /// </summary>
        /// <param name="id">Идентификатор задачи</param>
        /// <returns>Рабочая задача или сообщение об ошибке</returns>
        Task<ApiResponse<WorkTask>> GetTaskByIdAsync(int id);

        /// <summary>
        /// Создать новую рабочую задачу
        /// </summary>
        /// <param name="task">Данные рабочей задачи для создания</param>
        /// <returns>Созданная задача или сообщение об ошибке</returns>
        Task<ApiResponse<WorkTask>> CreateTaskAsync(WorkTask task);

        /// <summary>
        /// Обновить существующую рабочую задачу
        /// </summary>
        /// <param name="id">Идентификатор задачи</param>
        /// <param name="task">Обновленные данные задачи</param>
        /// <returns>Результат операции обновления</returns>
        Task<ApiResponse<WorkTask>> UpdateTaskAsync(int id, WorkTask task);

        /// <summary>
        /// Удалить рабочую задачу
        /// </summary>
        /// <param name="id">Идентификатор задачи</param>
        /// <returns>Результат операции удаления</returns>
        Task<ApiResponse<bool>> DeleteTaskAsync(int id);
    }
}
