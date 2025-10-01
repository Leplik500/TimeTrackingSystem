using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeTrackingAPI.Common.DTOs;
using TimeTrackingAPI.Common.Enums;
using TimeTrackingAPI.Common.Models;
using TimeTrackingAPI.Data;
using TimeTrackingAPI.Models;
using TimeTrackingAPI.Services.Interfaces;

namespace TimeTrackingAPI.Controllers;

/// <summary>
///     Контроллер для управления проводками времени
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class TimeEntriesController(
        ApplicationDbContext context,
        ITimeEntryService timeEntryService,
        ILogger<TimeEntriesController> logger) : ControllerBase
{
    /// <summary>
    ///     Получить все проводки времени
    /// </summary>
    /// <returns>Список всех проводок времени с включением задачи и проекта</returns>
    /// <response code="200">Список проводок времени успешно получен</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<TimeEntry>>),
            200)]
    [ProducesResponseType(typeof(ApiResponse<List<TimeEntry>>),
            500)]
    public async Task<ActionResult<ApiResponse<List<TimeEntry>>>>
            GetTimeEntries()
    {
        logger.LogInformation(
                "Получен запрос на получение всех проводок времени");

        var response =
                await timeEntryService.GetAllTimeEntriesAsync();

        return StatusCode((int) response.StatusCode, response);
    }

    /// <summary>
    ///     Получить проводки за конкретную дату
    /// </summary>
    /// <param name="date">Дата в формате YYYY-MM-DD</param>
    /// <returns>Список проводок за указанную дату</returns>
    /// <response code="200">Список проводок за дату получен</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet("date/{date:datetime}")]
    [ProducesResponseType(typeof(ApiResponse<List<TimeEntry>>),
            200)]
    [ProducesResponseType(typeof(ApiResponse<List<TimeEntry>>),
            500)]
    public async Task<ActionResult<ApiResponse<List<TimeEntry>>>>
            GetTimeEntriesByDate(DateTime date)
    {
        logger.LogInformation(
                "Получен запрос на получение проводок за дату: {Date}",
                date.ToString("yyyy-MM-dd"));

        var response =
                await timeEntryService.GetTimeEntriesByDateAsync(
                        date);

        return StatusCode((int) response.StatusCode, response);
    }

    /// <summary>
    ///     Получить проводки за месяц
    /// </summary>
    /// <param name="year">Год</param>
    /// <param name="month">Месяц (1-12)</param>
    /// <returns>Список проводок за указанный месяц</returns>
    /// <response code="200">Список проводок за месяц получен</response>
    /// <response code="400">Некорректные параметры года или месяца</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet("month/{year:int}/{month:int}")]
    [ProducesResponseType(typeof(ApiResponse<List<TimeEntry>>),
            200)]
    [ProducesResponseType(typeof(ApiResponse<List<TimeEntry>>),
            400)]
    [ProducesResponseType(typeof(ApiResponse<List<TimeEntry>>),
            500)]
    public async Task<ActionResult<ApiResponse<List<TimeEntry>>>>
            GetTimeEntriesByMonth(int year, int month)
    {
        logger.LogInformation(
                "Получен запрос на получение проводок за месяц: {Year}-{Month:00}",
                year, month);

        if (month is < 1 or > 12)
            return BadRequest(ApiResponse<List<TimeEntry>>.Error(
                    ApiStatusCode.BadRequest,
                    "Месяц должен быть от 1 до 12"));

        if (year is < 1900 or > 2100)
            return BadRequest(ApiResponse<List<TimeEntry>>.Error(
                    ApiStatusCode.BadRequest,
                    "Год должен быть от 1900 до 2100"));

        var response =
                await timeEntryService
                        .GetTimeEntriesByMonthAsync(year, month);

        return StatusCode((int) response.StatusCode, response);
    }

    /// <summary>
    ///     Создать новую проводку времени
    /// </summary>
    /// <param name="timeEntryDto">Данные для создания проводки</param>
    /// <returns>Созданная проводка времени</returns>
    /// <response code="201">Проводка времени успешно создана</response>
    /// <response code="400">Некорректные данные запроса или нарушения валидации</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TimeEntry>), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ApiResponse<TimeEntry>), 500)]
    public async Task<ActionResult<ApiResponse<TimeEntry>>>
            CreateTimeEntry(TimeEntryCreateDto timeEntryDto)
    {
        logger.LogInformation(
                "Получен запрос на создание проводки времени на дату {Date} для задачи {TaskId}",
                timeEntryDto.Date.ToString("yyyy-MM-dd"),
                timeEntryDto.TaskId);

        // Проверка активности задачи
        var task = await context.WorkTasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t =>
                        t.Id == timeEntryDto.TaskId);

        if (task == null)
            return BadRequest(ApiResponse<TimeEntry>.Error(
                    ApiStatusCode.BadRequest,
                    $"Задача с ID {timeEntryDto.TaskId} не найдена"));

        if (!task.IsActive)
            return BadRequest(ApiResponse<TimeEntry>.Error(
                    ApiStatusCode.BadRequest,
                    $"Задача '{task.Name}' неактивна. Нельзя создавать проводки для неактивных задач"));

        // Валидация дневных часов
        var validationResult =
                await ValidateDailyHours(timeEntryDto.Date,
                        timeEntryDto.Hours);

        if (!validationResult.IsValid)
            return BadRequest(ApiResponse<TimeEntry>.Error(
                    ApiStatusCode.BadRequest,
                    validationResult.ErrorMessage));

        var timeEntry = new TimeEntry
        {
            Date = timeEntryDto.Date.Date,
            Hours = timeEntryDto.Hours,
            Description = timeEntryDto.Description,
            TaskId = timeEntryDto.TaskId,
            Task = null! // Будет загружена в сервисе
        };

        var response =
                await timeEntryService.CreateTimeEntryAsync(
                        timeEntry);

        if (response.IsSuccess)
            return CreatedAtAction(
                    nameof(GetTimeEntries),
                    response);

        return StatusCode((int) response.StatusCode, response);
    }

    /// <summary>
    ///     Получить суммарную информацию о часах по дням
    /// </summary>
    /// <returns>Список дневных сводок с суммой часов и статусом</returns>
    /// <response code="200">Дневная сводка получена</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet("daily-summary")]
    [ProducesResponseType(
            typeof(ApiResponse<List<DailyHoursSummaryDto>>),
            200)]
    [ProducesResponseType(
            typeof(ApiResponse<List<DailyHoursSummaryDto>>),
            500)]
    public async
            Task<ActionResult<
                    ApiResponse<List<DailyHoursSummaryDto>>>>
            GetDailySummary()
    {
        logger.LogInformation(
                "Получен запрос на получение дневной сводки часов");

        var response =
                await timeEntryService.GetDailySummaryAsync();

        return StatusCode((int) response.StatusCode, response);
    }

    /// <summary>
    ///     Валидация суммы часов за день
    /// </summary>
    /// <param name="date">Дата</param>
    /// <param name="hours">Количество часов для добавления</param>
    /// <param name="excludeId">ID записи для исключения из проверки (при обновлении)</param>
    /// <returns>Результат валидации</returns>
    private async Task<(bool IsValid, string ErrorMessage)>
            ValidateDailyHours(DateTime date,
                    decimal hours,
                    int? excludeId = null)
    {
        try
        {
            logger.LogInformation(
                    "Валидация дневных часов для даты {Date}, добавляемых часов {Hours}",
                    date.ToString("yyyy-MM-dd"), hours);

            var dateOnly = date.Date;
            var query = context.TimeEntries
                    .AsNoTracking()
                    .Where(te => te.Date.Date == dateOnly);

            if (excludeId.HasValue)
                query =
                        query.Where(te =>
                                te.Id != excludeId.Value);

            var existingHours =
                    await query.SumAsync(te => te.Hours);

            var totalHours = existingHours + hours;

            logger.LogInformation(
                    "Существующие часы: {ExistingHours}, добавляемые: {Hours}, итого: {TotalHours}",
                    existingHours, hours, totalHours);

            if (totalHours > 24)
                return (false,
                        $"Сумма часов за день {date:yyyy-MM-dd} не может превышать 24 часа. " +
                        $"Текущая сумма: {existingHours}, добавляется: {hours}, итого: {totalHours}");

            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                    "Ошибка при валидации дневных часов");

            return (false, "Ошибка при проверке дневных часов");
        }
    }
}