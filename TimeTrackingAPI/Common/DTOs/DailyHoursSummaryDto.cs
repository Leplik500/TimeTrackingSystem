namespace TimeTrackingAPI.Common.DTOs;

/// <summary>
///     DTO для передачи агрегированных данных о часах по дням
/// </summary>
/// <remarks>
///     Используется в GET /api/timeentries/daily-summary для возврата статистики.
///     Статус рассчитывается автоматически: insufficient (&lt; 8 часов),
///     sufficient (= 8 часов), excessive (&gt; 8 часов).
/// </remarks>
/// <example>
///     {
///     "date": "2025-01-15T00:00:00Z",
///     "totalHours": 8.5,
///     "status": "excessive"
///     }
/// </example>
public sealed record DailyHoursSummaryDto
{
    /// <summary>
    ///     Дата
    /// </summary>
    /// <example>2025-01-15T00:00:00Z</example>
    public DateTime Date { get; init; }

    /// <summary>
    ///     Общее количество часов за день
    /// </summary>
    /// <example>8.5</example>
    public decimal TotalHours { get; init; }

    /// <summary>
    ///     Статус дня: "insufficient" (менее 8 часов), "sufficient" (8 часов),
    ///     "excessive" (более 8 часов)
    /// </summary>
    /// <example>excessive</example>
    public string Status { get; init; } = string.Empty;
}