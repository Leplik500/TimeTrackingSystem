using Microsoft.EntityFrameworkCore;
using TimeTrackingAPI.Common.DTOs;
using TimeTrackingAPI.Common.Enums;
using TimeTrackingAPI.Common.Models;
using TimeTrackingAPI.Data;
using TimeTrackingAPI.Models;
using TimeTrackingAPI.Services.Interfaces;

namespace TimeTrackingAPI.Services;

/// <summary>
///     Сервис для работы с проводками времени
/// </summary>
public sealed class TimeEntryService(
        ApplicationDbContext context,
        ILogger<TimeEntryService> logger) : ITimeEntryService
{
    /// <summary>
    ///     Получить все проводки времени
    /// </summary>
    /// <returns>Ответ API со списком проводок времени</returns>
    public async Task<ApiResponse<List<TimeEntry>>>
            GetAllTimeEntriesAsync()
    {
        try
        {
            logger.LogInformation(
                    "Запрос на получение всех проводок времени");

            var timeEntries = await context.TimeEntries
                    .AsNoTracking()
                    .Include(timeEntry => timeEntry.Task.Project)
                    .OrderByDescending(te => te.Date)
                    .ThenBy(te => te.Id)
                    .ToListAsync();

            logger.LogInformation(
                    "Получено {Count} проводок времени",
                    timeEntries.Count);

            return ApiResponse<List<TimeEntry>>.Success(
                    timeEntries,
                    $"Получено {timeEntries.Count} проводок времени");
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                    "Ошибка при получении списка проводок времени");

            return ApiResponse<List<TimeEntry>>.Error(
                    ApiStatusCode.InternalServerError,
                    $"Произошла ошибка при получении списка проводок времени: {ex.Message}");
        }
    }

    /// <summary>
    ///     Получить проводки за конкретную дату
    /// </summary>
    /// <param name="date">Дата</param>
    /// <returns>Ответ API со списком проводок за указанную дату</returns>
    public async Task<ApiResponse<List<TimeEntry>>>
            GetTimeEntriesByDateAsync(DateTime date)
    {
        try
        {
            logger.LogInformation(
                    "Запрос проводок за дату: {Date}",
                    date.ToString("yyyy-MM-dd"));

            var dateOnly = date.Date;
            var timeEntries = await context.TimeEntries
                    .AsNoTracking()
                    .Include(timeEntry => timeEntry.Task.Project)
                    .Where(te => te.Date.Date == dateOnly)
                    .OrderBy(te => te.Id)
                    .ToListAsync();

            logger.LogInformation(
                    "Получено {Count} проводок за дату {Date}",
                    timeEntries.Count,
                    date.ToString("yyyy-MM-dd"));

            return ApiResponse<List<TimeEntry>>.Success(
                    timeEntries,
                    $"Получено {timeEntries.Count} проводок за {date:yyyy-MM-dd}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                    "Ошибка при получении проводок за дату {Date}",
                    date);

            return ApiResponse<List<TimeEntry>>.Error(
                    ApiStatusCode.InternalServerError,
                    $"Произошла ошибка при получении проводок за дату: {ex.Message}");
        }
    }

    /// <summary>
    ///     Получить проводки за месяц
    /// </summary>
    /// <param name="year">Год</param>
    /// <param name="month">Месяц</param>
    /// <returns>Ответ API со списком проводок за указанный месяц</returns>
    public async Task<ApiResponse<List<TimeEntry>>>
            GetTimeEntriesByMonthAsync(int year, int month)
    {
        try
        {
            logger.LogInformation(
                    "Запрос проводок за месяц: {Year}-{Month:00}",
                    year, month);

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            var timeEntries = await context.TimeEntries
                    .AsNoTracking()
                    .Include(timeEntry => timeEntry.Task.Project)
                    .Where(te =>
                            te.Date >= startDate &&
                            te.Date < endDate)
                    .OrderByDescending(te => te.Date)
                    .ThenBy(te => te.Id)
                    .ToListAsync();

            logger.LogInformation(
                    "Получено {Count} проводок за {Year}-{Month:00}",
                    timeEntries.Count, year, month);

            return ApiResponse<List<TimeEntry>>.Success(
                    timeEntries,
                    $"Получено {timeEntries.Count} проводок за {year:0000}-{month:00}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                    "Ошибка при получении проводок за месяц {Year}-{Month}",
                    year, month);

            return ApiResponse<List<TimeEntry>>.Error(
                    ApiStatusCode.InternalServerError,
                    $"Произошла ошибка при получении проводок за месяц: {ex.Message}");
        }
    }

    /// <summary>
    ///     Создать новую проводку времени
    /// </summary>
    /// <param name="timeEntry">Данные проводки</param>
    /// <returns>Ответ API с созданной проводкой</returns>
    public async Task<ApiResponse<TimeEntry>>
            CreateTimeEntryAsync(TimeEntry timeEntry)
    {
        try
        {
            logger.LogInformation(
                    "Создание новой проводки времени на дату {Date} для задачи {TaskId}",
                    timeEntry.Date.ToString("yyyy-MM-dd"),
                    timeEntry.TaskId);

            // Добавляем проводку в контекст
            context.TimeEntries.Add(timeEntry);
            await context.SaveChangesAsync();

            // Загружаем связанные данные для ответа
            await context.Entry(timeEntry)
                    .Reference(te => te.Task)
                    .LoadAsync();

            await context.Entry(timeEntry.Task)
                    .Reference(t => t.Project)
                    .LoadAsync();

            logger.LogInformation(
                    "Проводка времени успешно создана с ID: {Id}",
                    timeEntry.Id);

            return ApiResponse<TimeEntry>.Success(
                    timeEntry,
                    "Проводка времени успешно создана");
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                    "Ошибка при создании проводки времени");

            return ApiResponse<TimeEntry>.Error(
                    ApiStatusCode.InternalServerError,
                    $"Произошла ошибка при создании проводки времени: {ex.Message}");
        }
    }

    /// <summary>
    ///     Получить суммарную информацию о часах по дням
    /// </summary>
    /// <returns>Ответ API со списком дневных сводок</returns>
    public async Task<ApiResponse<List<DailyHoursSummaryDto>>>
            GetDailySummaryAsync()
    {
        try
        {
            logger.LogInformation(
                    "Запрос суммарной информации о часах по дням");

            var dailySummary = await context.TimeEntries
                    .AsNoTracking()
                    .GroupBy(te => te.Date.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        TotalHours = g.Sum(te => te.Hours)
                    })
                    .OrderByDescending(ds => ds.Date)
                    .ToListAsync();

            var result = dailySummary.Select(ds =>
                            new DailyHoursSummaryDto
                            {
                                Date = ds.Date,
                                TotalHours = ds.TotalHours,
                                Status =
                                        GetDailyStatus(
                                                ds.TotalHours)
                            })
                    .ToList();

            logger.LogInformation(
                    "Получена сводка по {Count} дням",
                    result.Count);

            return ApiResponse<List<DailyHoursSummaryDto>>
                    .Success(
                            result,
                            $"Получена сводка по {result.Count} дням");
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                    "Ошибка при получении дневной сводки");

            return ApiResponse<List<DailyHoursSummaryDto>>.Error(
                    ApiStatusCode.InternalServerError,
                    $"Произошла ошибка при получении дневной сводки: {ex.Message}");
        }
    }

    /// <summary>
    ///     Получить статус дня на основе суммы часов
    /// </summary>
    /// <param name="totalHours">Общее количество часов за день</param>
    /// <returns>
    ///     Строковый статус: "insufficient" если менее 8 часов,
    ///     "sufficient" если ровно 8 часов,
    ///     "excessive" если более 8 часов
    /// </returns>
    private static string GetDailyStatus(decimal totalHours)
    {
        return totalHours switch
        {
            < 8m => "insufficient",
            8m => "sufficient",
            > 8m => "excessive"
        };
    }
}