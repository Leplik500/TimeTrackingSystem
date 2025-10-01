namespace TimeTrackingAPI.Common.Enums;

/// <summary>
///     Перечисление кодов состояния API
/// </summary>
public enum ApiStatusCode
{
    /// <summary>
    ///     Успешное выполнение
    /// </summary>
    Success = 200,

    /// <summary>
    ///     Ресурс не найден
    /// </summary>
    NotFound = 404,

    /// <summary>
    ///     Внутренняя ошибка сервера
    /// </summary>
    InternalServerError = 500,

    /// <summary>
    ///     Некорректные данные запроса
    /// </summary>
    BadRequest = 400
}