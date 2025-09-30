using TimeTrackingAPI.Common.Models;
using TimeTrackingAPI.Models;

namespace TimeTrackingAPI.Services.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса для работы с проектами
    /// </summary>
    public interface IProjectService
    {
        /// <summary>
        /// Получить все проекты
        /// </summary>
        /// <returns>Список всех проектов</returns>
        Task<ApiResponse<List<Project>>> GetAllProjectsAsync();

        /// <summary>
        /// Получить проект по ID
        /// </summary>
        /// <param name="id">Идентификатор проекта</param>
        /// <returns>Проект или сообщение об ошибке</returns>
        Task<ApiResponse<Project>> GetProjectByIdAsync(int id);

        /// <summary>
        /// Создать новый проект
        /// </summary>
        /// <param name="project">Данные проекта для создания</param>
        /// <returns>Созданный проект или сообщение об ошибке</returns>
        Task<ApiResponse<Project>> CreateProjectAsync(Project project);

        /// <summary>
        /// Обновить существующий проект
        /// </summary>
        /// <param name="id">Идентификатор проекта</param>
        /// <param name="project">Обновленные данные проекта</param>
        /// <returns>Результат операции обновления</returns>
        Task<ApiResponse<Project>> UpdateProjectAsync(int id, Project project);

        /// <summary>
        /// Удалить проект
        /// </summary>
        /// <param name="id">Идентификатор проекта</param>
        /// <returns>Результат операции удаления</returns>
        Task<ApiResponse<bool>> DeleteProjectAsync(int id);
    }
}