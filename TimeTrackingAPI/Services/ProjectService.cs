using Microsoft.EntityFrameworkCore;
using TimeTrackingAPI.Common.Enums;
using TimeTrackingAPI.Common.Models;
using TimeTrackingAPI.Data;
using TimeTrackingAPI.Models;
using TimeTrackingAPI.Services.Interfaces;

namespace TimeTrackingAPI.Services
{
    /// <summary>
    /// Сервис для работы с проектами
    /// </summary>
    public sealed class ProjectService(
            ApplicationDbContext context,
            ILogger<ProjectService> logger) : IProjectService
    {
        /// <summary>
        /// Получить все проекты
        /// </summary>
        /// <returns>Ответ API со списком проектов</returns>
        public async Task<ApiResponse<List<Project>>> GetAllProjectsAsync()
        {
            try
            {
                logger.LogInformation("Запрос на получение всех проектов");
                
                var projects = await context.Projects
                        .AsNoTracking()
                        .OrderBy(p => p.Name)
                        .ToListAsync();
                
                logger.LogInformation("Получено {Count} проектов", projects.Count);
                
                return ApiResponse<List<Project>>.Success(
                        projects, 
                        $"Получено {projects.Count} проектов");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при получении списка проектов");
                
                return ApiResponse<List<Project>>.Error(
                        ApiStatusCode.InternalServerError,
                        $"Произошла ошибка при получении списка проектов: {ex.Message}");
            }
        }
    }
}