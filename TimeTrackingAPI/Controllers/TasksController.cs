using Microsoft.AspNetCore.Mvc;
using TimeTrackingAPI.Common.DTOs;
using TimeTrackingAPI.Common.Models;
using TimeTrackingAPI.Models;
using TimeTrackingAPI.Services.Interfaces;

namespace TimeTrackingAPI.Controllers;

/// <summary>
///     Контроллер для работы с рабочими задачами
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class TasksController(
        ITaskService taskService,
        ILogger<TasksController> logger) : ControllerBase
{
    /// <summary>
    ///     Получить все рабочие задачи
    /// </summary>
    /// <returns>Список всех рабочих задач</returns>
    /// <response code="200">Список задач успешно получен</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<WorkTask>>),
            200)]
    [ProducesResponseType(typeof(ApiResponse<List<WorkTask>>),
            500)]
    public async Task<ActionResult<ApiResponse<List<WorkTask>>>>
            GetTasks()
    {
        logger.LogInformation(
                "Получен запрос на получение всех задач");

        var response = await taskService.GetAllTasksAsync();

        return StatusCode((int) response.StatusCode, response);
    }

    /// <summary>
    ///     Получить только активные рабочие задачи
    /// </summary>
    /// <returns>Список активных рабочих задач</returns>
    /// <response code="200">Список активных задач успешно получен</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    /// <remarks>
    ///     Возвращает только задачи со статусом IsActive = true.
    ///     Полезно для получения задач, доступных для учета времени.
    /// </remarks>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<List<WorkTask>>),
            200)]
    [ProducesResponseType(typeof(ApiResponse<List<WorkTask>>),
            500)]
    public async Task<ActionResult<ApiResponse<List<WorkTask>>>>
            GetActiveTasks()
    {
        logger.LogInformation(
                "Получен запрос на получение только активных задач");

        var response = await taskService.GetActiveTasksAsync();

        return StatusCode((int) response.StatusCode, response);
    }

    /// <summary>
    ///     Получить рабочую задачу по ID
    /// </summary>
    /// <param name="id">Идентификатор задачи</param>
    /// <returns>Найденная задача или сообщение об ошибке</returns>
    /// <response code="200">Задача найдена</response>
    /// <response code="404">Задача не найдена</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<WorkTask>), 200)]
    [ProducesResponseType(typeof(ApiResponse<WorkTask>), 404)]
    [ProducesResponseType(typeof(ApiResponse<WorkTask>), 500)]
    public async Task<ActionResult<ApiResponse<WorkTask>>>
            GetTask(int id)
    {
        logger.LogInformation(
                "Получен запрос на получение задачи с ID: {Id}",
                id);

        var response = await taskService.GetTaskByIdAsync(id);

        return StatusCode((int) response.StatusCode, response);
    }

    /// <summary>
    ///     Создать новую рабочую задачу
    /// </summary>
    /// <param name="taskDto">Данные для создания задачи</param>
    /// <returns>Созданная задача</returns>
    /// <response code="201">Задача успешно создана</response>
    /// <response code="400">
    ///     Некорректные данные запроса или задача с таким названием
    ///     уже существует в проекте
    /// </response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<WorkTask>), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ApiResponse<WorkTask>), 500)]
    public async Task<ActionResult<ApiResponse<WorkTask>>>
            CreateTask(TaskCreateDto taskDto)
    {
        logger.LogInformation(
                "Получен запрос на создание задачи: {Name} для проекта {ProjectId}",
                taskDto.Name, taskDto.ProjectId);

        var task = new WorkTask
        {
            Name = taskDto.Name,
            ProjectId = taskDto.ProjectId,
            IsActive = taskDto.IsActive,
            Project = null! // Будет загружен в сервисе
        };

        var response = await taskService.CreateTaskAsync(task);

        if (response.IsSuccess)
            return CreatedAtAction(
                    nameof(GetTask),
                    new {id = response.Data!.Id},
                    response);

        return StatusCode((int) response.StatusCode, response);
    }

    /// <summary>
    ///     Обновить существующую рабочую задачу
    /// </summary>
    /// <param name="id">Идентификатор задачи</param>
    /// <param name="taskDto">Обновленные данные задачи</param>
    /// <returns>Результат обновления</returns>
    /// <response code="200">Задача успешно обновлена</response>
    /// <response code="400">Некорректные данные запроса</response>
    /// <response code="404">Задача не найдена</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<WorkTask>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ApiResponse<WorkTask>), 404)]
    [ProducesResponseType(typeof(ApiResponse<WorkTask>), 500)]
    public async Task<ActionResult<ApiResponse<WorkTask>>>
            UpdateTask(int id, TaskUpdateDto taskDto)
    {
        logger.LogInformation(
                "Получен запрос на обновление задачи с ID: {Id}",
                id);

        var task = new WorkTask
        {
            Id = id,
            Name = taskDto.Name,
            ProjectId = taskDto.ProjectId,
            IsActive = taskDto.IsActive,
            Project = null! // Будет загружен в сервисе
        };

        var response =
                await taskService.UpdateTaskAsync(id, task);

        return StatusCode((int) response.StatusCode, response);
    }

    /// <summary>
    ///     Удалить рабочую задачу
    /// </summary>
    /// <param name="id">Идентификатор задачи</param>
    /// <returns>Результат удаления</returns>
    /// <response code="200">Задача успешно удалена</response>
    /// <response code="400">
    ///     Задачу нельзя удалить из-за наличия связанных записей
    ///     времени
    /// </response>
    /// <response code="404">Задача не найдена</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 500)]
    public async Task<ActionResult<ApiResponse<bool>>>
            DeleteTask(int id)
    {
        logger.LogInformation(
                "Получен запрос на удаление задачи с ID: {Id}",
                id);

        var response = await taskService.DeleteTaskAsync(id);

        return StatusCode((int) response.StatusCode, response);
    }
}