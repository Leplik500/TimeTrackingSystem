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
    }
}