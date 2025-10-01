using TimeTrackingAPI.Common.DTOs;
using TimeTrackingAPI.Common.Models;
using TimeTrackingAPI.Models;

namespace TimeTrackingAPI.Services.Interfaces;

/// <summary>
/// Интерфейс сервиса для работы с проводками времени
/// </summary>
public interface ITimeEntryService
{
    /// <summary>
    /// Получить все проводки времени
    /// </summary>
    /// <returns>Список всех проводок времени</returns>
    Task<ApiResponse<List<TimeEntry>>> GetAllTimeEntriesAsync();

    /// <summary>
    /// Получить проводки за конкретную дату
    /// </summary>
    /// <param name="date">Дата</param>
    /// <returns>Список проводок за указанную дату</returns>
    Task<ApiResponse<List<TimeEntry>>> GetTimeEntriesByDateAsync(DateTime date);

    /// <summary>
    /// Получить проводки за месяц
    /// </summary>
    /// <param name="year">Год</param>
    /// <param name="month">Месяц</param>
    /// <returns>Список проводок за указанный месяц</returns>
    Task<ApiResponse<List<TimeEntry>>> GetTimeEntriesByMonthAsync(int year, int month);

    /// <summary>
    /// Создать новую проводку времени
    /// </summary>
    /// <param name="timeEntry">Данные проводки</param>
    /// <returns>Созданная проводка</returns>
    Task<ApiResponse<TimeEntry>> CreateTimeEntryAsync(TimeEntry timeEntry);

    /// <summary>
    /// Получить суммарную информацию о часах по дням
    /// </summary>
    /// <returns>Список дневных сводок</returns>
    Task<ApiResponse<List<DailyHoursSummaryDto>>> GetDailySummaryAsync();
}