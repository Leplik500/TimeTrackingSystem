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

        /// <summary>
        /// Получить проект по ID
        /// </summary>
        /// <param name="id">Идентификатор проекта</param>
        /// <returns>Ответ API с найденным проектом</returns>
        public async Task<ApiResponse<Project>> GetProjectByIdAsync(int id)
        {
            try
            {
                logger.LogInformation("Запрос на получение проекта с ID: {Id}", id);
                
                var project = await context.Projects
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id);
                
                if (project == null)
                {
                    logger.LogWarning("Проект с ID {Id} не найден", id);
                    return ApiResponse<Project>.Error(
                        ApiStatusCode.NotFound,
                        $"Проект с ID {id} не найден");
                }
                
                logger.LogInformation("Проект с ID {Id} найден: {Name}", id, project.Name);
                
                return ApiResponse<Project>.Success(
                    project,
                    "Проект успешно получен");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при получении проекта с ID {Id}", id);
                
                return ApiResponse<Project>.Error(
                    ApiStatusCode.InternalServerError,
                    $"Произошла ошибка при получении проекта: {ex.Message}");
            }
        }

        /// <summary>
        /// Создать новый проект
        /// </summary>
        /// <param name="project">Данные нового проекта</param>
        /// <returns>Ответ API с созданным проектом</returns>
        public async Task<ApiResponse<Project>> CreateProjectAsync(Project project)
        {
            try
            {
                logger.LogInformation("Создание нового проекта: {Name}", project.Name);
                
                // Проверка на дублирование кода проекта
                var existingProject = await context.Projects
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Code == project.Code);
                
                if (existingProject != null)
                {
                    logger.LogWarning("Проект с кодом {Code} уже существует", project.Code);
                    return ApiResponse<Project>.Error(
                        ApiStatusCode.BadRequest,
                        $"Проект с кодом '{project.Code}' уже существует");
                }
                
                // Добавление проекта в контекст
                context.Projects.Add(project);
                await context.SaveChangesAsync();
                
                logger.LogInformation("Проект успешно создан с ID: {Id}", project.Id);
                
                return ApiResponse<Project>.Success(
                    project,
                    "Проект успешно создан");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при создании проекта");
                
                return ApiResponse<Project>.Error(
                    ApiStatusCode.InternalServerError,
                    $"Произошла ошибка при создании проекта: {ex.Message}");
            }
        }

        /// <summary>
        /// Обновить существующий проект
        /// </summary>
        /// <param name="id">Идентификатор проекта</param>
        /// <param name="project">Обновленные данные проекта</param>
        /// <returns>Ответ API с обновленным проектом</returns>
        public async Task<ApiResponse<Project>> UpdateProjectAsync(int id, Project project)
        {
            try
            {
                logger.LogInformation("Обновление проекта с ID: {Id}", id);
                
                var existingProject = await context.Projects.FindAsync(id);
                if (existingProject == null)
                {
                    logger.LogWarning("Проект с ID {Id} не найден для обновления", id);
                    return ApiResponse<Project>.Error(
                        ApiStatusCode.NotFound,
                        $"Проект с ID {id} не найден");
                }
                
                // Проверка на дублирование кода проекта (исключая текущий проект)
                var duplicateProject = await context.Projects
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Code == project.Code && p.Id != id);
                
                if (duplicateProject != null)
                {
                    logger.LogWarning("Проект с кодом {Code} уже существует", project.Code);
                    return ApiResponse<Project>.Error(
                        ApiStatusCode.BadRequest,
                        $"Проект с кодом '{project.Code}' уже существует");
                }
                
                // Обновление всех полей
                existingProject.Name = project.Name;
                existingProject.Code = project.Code;
                existingProject.IsActive = project.IsActive;
                
                await context.SaveChangesAsync();
                
                logger.LogInformation("Проект с ID {Id} успешно обновлен", id);
                
                return ApiResponse<Project>.Success(
                    existingProject,
                    "Проект успешно обновлен");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при обновлении проекта с ID {Id}", id);
                
                return ApiResponse<Project>.Error(
                    ApiStatusCode.InternalServerError,
                    $"Произошла ошибка при обновлении проекта: {ex.Message}");
            }
        }

        /// <summary>
        /// Удалить проект
        /// </summary>
        /// <param name="id">Идентификатор проекта</param>
        /// <returns>Ответ API с результатом удаления</returns>
        public async Task<ApiResponse<bool>> DeleteProjectAsync(int id)
        {
            try
            {
                logger.LogInformation("Удаление проекта с ID: {Id}", id);
                
                var project = await context.Projects
                    .Include(p => p.Tasks)
                    .FirstOrDefaultAsync(p => p.Id == id);
                
                if (project == null)
                {
                    logger.LogWarning("Проект с ID {Id} не найден для удаления", id);
                    return ApiResponse<bool>.Error(
                        ApiStatusCode.NotFound,
                        $"Проект с ID {id} не найден");
                }
                
                // Проверка на наличие связанных задач
                if (project.Tasks.Any())
                {
                    logger.LogWarning("Нельзя удалить проект с ID {Id} - есть связанные задачи", id);
                    return ApiResponse<bool>.Error(
                        ApiStatusCode.BadRequest,
                        $"Нельзя удалить проект '{project.Name}' - у него есть {project.Tasks.Count} связанных задач");
                }
                
                context.Projects.Remove(project);
                await context.SaveChangesAsync();
                
                logger.LogInformation("Проект с ID {Id} успешно удален", id);
                
                return ApiResponse<bool>.Success(
                    true,
                    "Проект успешно удален");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при удалении проекта с ID {Id}", id);
                
                return ApiResponse<bool>.Error(
                    ApiStatusCode.InternalServerError,
                    $"Произошла ошибка при удалении проекта: {ex.Message}");
            }
        }
    }
}
