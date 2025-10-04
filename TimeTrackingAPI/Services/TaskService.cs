using Microsoft.EntityFrameworkCore;
using TimeTrackingAPI.Common.Enums;
using TimeTrackingAPI.Common.Models;
using TimeTrackingAPI.Data;
using TimeTrackingAPI.Models;
using TimeTrackingAPI.Services.Interfaces;

namespace TimeTrackingAPI.Services;

/// <summary>
///     Сервис для работы с рабочими задачами
/// </summary>
public sealed class TaskService(
        ApplicationDbContext context,
        ILogger<TaskService> logger) : ITaskService
{
        /// <summary>
        ///     Получить все рабочие задачи
        /// </summary>
        /// <returns>Ответ API со списком рабочих задач</returns>
        public async Task<ApiResponse<List<WorkTask>>>
            GetAllTasksAsync()
    {
        try
        {
            logger.LogInformation(
                    "Запрос на получение всех рабочих задач");

            var tasks = await context.WorkTasks
                    .Include(t => t.Project)
                    .OrderBy(t => t.Name)
                    .ToListAsync();

            logger.LogInformation(
                    "Получено {Count} рабочих задач",
                    tasks.Count);

            return ApiResponse<List<WorkTask>>.Success(
                    tasks,
                    $"Получено {tasks.Count} рабочих задач");
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                    "Ошибка при получении списка рабочих задач");

            return ApiResponse<List<WorkTask>>.Error(
                    ApiStatusCode.InternalServerError,
                    $"Произошла ошибка при получении списка задач: {ex.Message}");
        }
    }

        /// <summary>
        ///     Получить рабочую задачу по ID
        /// </summary>
        /// <param name="id">Идентификатор задачи</param>
        /// <returns>Ответ API с найденной задачей</returns>
        public async Task<ApiResponse<WorkTask>>
                        GetTaskByIdAsync(
                                        int id)
    {
        try
        {
            logger.LogInformation(
                            "Запрос на получение задачи с ID: {Id}",
                            id);

            var task = await context.WorkTasks
                    .Include(t => t.Project)
                    .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                    logger.LogWarning(
                                    "Задача с ID {Id} не найдена",
                                    id);

                return ApiResponse<WorkTask>.Error(
                        ApiStatusCode.NotFound,
                        $"Задача с ID {id} не найдена");
            }

            logger.LogInformation(
                    "Задача с ID {Id} найдена: {Name}", id,
                    task.Name);

            return ApiResponse<WorkTask>.Success(task,
                    "Задача успешно получена");
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                            "Ошибка при получении задачи с ID {Id}",
                            id);

            return ApiResponse<WorkTask>.Error(
                    ApiStatusCode.InternalServerError,
                    $"Произошла ошибка при получении задачи: {ex.Message}");
        }
    }

        /// <summary>
        ///     Создать новую рабочую задачу
        /// </summary>
        /// <param name="task">Данные новой задачи</param>
        /// <returns>Ответ API с созданной задачей</returns>
        public async Task<ApiResponse<WorkTask>> CreateTaskAsync(
            WorkTask task)
    {
        try
        {
            logger.LogInformation(
                    "Создание новой задачи: {Name} для проекта {ProjectId}",
                    task.Name, task.ProjectId);

            var projectExists = await context.Projects
                    .AnyAsync(p => p.Id == task.ProjectId);

            if (!projectExists)
            {
                logger.LogWarning(
                        "Проект с ID {ProjectId} не существует",
                        task.ProjectId);

                return ApiResponse<WorkTask>.Error(
                        ApiStatusCode.BadRequest,
                        $"Проект с ID {task.ProjectId} не существует");
            }

            var existingTask = await context.WorkTasks
                    .FirstOrDefaultAsync(t =>
                            t.Name == task.Name &&
                            t.ProjectId == task.ProjectId);

            if (existingTask != null)
            {
                logger.LogWarning(
                        "Задача с названием '{Name}' уже существует в проекте {ProjectId}",
                        task.Name, task.ProjectId);

                return ApiResponse<WorkTask>.Error(
                        ApiStatusCode.BadRequest,
                        $"Задача с названием '{task.Name}' уже существует в данном проекте");
            }

            // Добавление задачи в контекст
            context.WorkTasks.Add(task);
            await context.SaveChangesAsync();

            // Загружаем связанные данные для ответа
            await context.Entry(task)
                    .Reference(t => t.Project)
                    .LoadAsync();

            logger.LogInformation(
                    "Задача успешно создана с ID: {Id}",
                    task.Id);

            return ApiResponse<WorkTask>.Success(task,
                    "Задача успешно создана");
        }
        catch (Exception ex)
        {
                logger.LogError(ex,
                                "Ошибка при создании задачи");

            return ApiResponse<WorkTask>.Error(
                    ApiStatusCode.InternalServerError,
                    $"Произошла ошибка при создании задачи: {ex.Message}");
        }
    }

        /// <summary>
        ///     Обновить существующую рабочую задачу
        /// </summary>
        /// <param name="id">Идентификатор задачи</param>
        /// <param name="task">Обновленные данные задачи</param>
        /// <returns>Ответ API с обновленной задачей</returns>
        public async Task<ApiResponse<WorkTask>> UpdateTaskAsync(
            int id,
            WorkTask task)
    {
        try
        {
                logger.LogInformation(
                                "Обновление задачи с ID: {Id}",
                                id);

            var existingTask = await context.WorkTasks
                            .AsTracking()
                    .Include(t => t.Project)
                    .FirstOrDefaultAsync(t => t.Id == id);

            if (existingTask == null)
            {
                logger.LogWarning(
                        "Задача с ID {Id} не найдена для обновления",
                        id);

                return ApiResponse<WorkTask>.Error(
                        ApiStatusCode.NotFound,
                        $"Задача с ID {id} не найдена");
            }

            var projectExists = await context.Projects
                    .AnyAsync(p => p.Id == task.ProjectId);

            if (!projectExists)
            {
                logger.LogWarning(
                        "Проект с ID {ProjectId} не существует",
                        task.ProjectId);

                return ApiResponse<WorkTask>.Error(
                        ApiStatusCode.BadRequest,
                        $"Проект с ID {task.ProjectId} не существует");
            }

            // Проверка на дублирование названия задачи в рамках проекта (исключая текущую)
            var duplicateTask = await context.WorkTasks
                    .FirstOrDefaultAsync(t =>
                            t.Name == task.Name &&
                            t.ProjectId == task.ProjectId &&
                            t.Id != id);

            if (duplicateTask != null)
            {
                logger.LogWarning(
                        "Задача с названием '{Name}' уже существует в проекте {ProjectId}",
                        task.Name, task.ProjectId);

                return ApiResponse<WorkTask>.Error(
                        ApiStatusCode.BadRequest,
                        $"Задача с названием '{task.Name}' уже существует в данном проекте");
            }

            existingTask.Name = task.Name;
            existingTask.ProjectId = task.ProjectId;
            existingTask.IsActive = task.IsActive;

            await context.SaveChangesAsync();

            // Перезагружаем связанные данные после обновления
            await context.Entry(existingTask)
                    .Reference(t => t.Project)
                    .LoadAsync();

            logger.LogInformation(
                            "Задача с ID {Id} успешно обновлена",
                            id);

            return ApiResponse<WorkTask>.Success(
                    existingTask,
                    "Задача успешно обновлена");
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                    "Ошибка при обновлении задачи с ID {Id}",
                    id);

            return ApiResponse<WorkTask>.Error(
                    ApiStatusCode.InternalServerError,
                    $"Произошла ошибка при обновлении задачи: {ex.Message}");
        }
    }

        /// <summary>
        ///     Удалить рабочую задачу
        /// </summary>
        /// <param name="id">Идентификатор задачи</param>
        /// <returns>Ответ API с результатом удаления</returns>
        public async Task<ApiResponse<bool>> DeleteTaskAsync(
                        int id)
    {
        try
        {
                logger.LogInformation(
                                "Удаление задачи с ID: {Id}",
                                id);

            var task = await context.WorkTasks
                            .AsTracking()
                    .Include(t => t.TimeEntries)
                    .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                logger.LogWarning(
                        "Задача с ID {Id} не найдена для удаления",
                        id);

                return ApiResponse<bool>.Error(
                        ApiStatusCode.NotFound,
                        $"Задача с ID {id} не найдена");
            }

            // Проверка на наличие связанных записей времени
            if (task.TimeEntries.Count != 0)
            {
                logger.LogWarning(
                        "Нельзя удалить задачу с ID {Id} - есть связанные записи времени",
                        id);

                return ApiResponse<bool>.Error(
                        ApiStatusCode.BadRequest,
                        $"Нельзя удалить задачу '{task.Name}' - у неё есть {task.TimeEntries.Count} связанных записей времени");
            }

            context.WorkTasks.Remove(task);
            await context.SaveChangesAsync();

            logger.LogInformation(
                    "Задача с ID {Id} успешно удалена", id);

            return ApiResponse<bool>.Success(true,
                    "Задача успешно удалена");
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                            "Ошибка при удалении задачи с ID {Id}",
                            id);

            return ApiResponse<bool>.Error(
                    ApiStatusCode.InternalServerError,
                    $"Произошла ошибка при удалении задачи: {ex.Message}");
        }
    }

        /// <summary>
        ///     Получить только активные рабочие задачи
        /// </summary>
        /// <returns>Ответ API со списком активных рабочих задач</returns>
        public async Task<ApiResponse<List<WorkTask>>>
            GetActiveTasksAsync()
    {
        try
        {
            logger.LogInformation(
                    "Запрос на получение только активных рабочих задач");

            // Убираем AsNoTracking() - он уже установлен глобально
            var activeTasks = await context.WorkTasks
                    .Include(t => t.Project)
                    .Where(task => task.IsActive == true)
                    .OrderBy(t => t.Name)
                    .ToListAsync();

            logger.LogInformation(
                    "Получено {Count} активных рабочих задач",
                    activeTasks.Count);

            return ApiResponse<List<WorkTask>>.Success(
                    activeTasks,
                    $"Получено {activeTasks.Count} активных рабочих задач");
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                    "Ошибка при получении списка активных рабочих задач");

            return ApiResponse<List<WorkTask>>.Error(
                    ApiStatusCode.InternalServerError,
                    $"Произошла ошибка при получении списка активных задач: {ex.Message}");
        }
    }
}