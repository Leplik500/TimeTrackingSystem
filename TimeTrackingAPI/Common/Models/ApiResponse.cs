using TimeTrackingAPI.Common.Enums;

namespace TimeTrackingAPI.Common.Models;

/// <summary>
///     Универсальная модель ответа API
/// </summary>
/// <typeparam name="T">Тип возвращаемых данных</typeparam>
public sealed class ApiResponse<T>
{
    /// <summary>
    ///     Данные ответа
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    ///     Код состояния
    /// </summary>
    public ApiStatusCode StatusCode { get; init; }

    /// <summary>
    ///     Сообщение (описание ошибки или успеха)
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    ///     Признак успешности операции
    /// </summary>
    public bool IsSuccess
    {
        get => StatusCode == ApiStatusCode.Success;
    }

    /// <summary>
    ///     Создать успешный ответ
    /// </summary>
    public static ApiResponse<T> Success(T data,
            string message = "Операция выполнена успешно")
    {
        return new ApiResponse<T>
        {
            Data = data,
            StatusCode = ApiStatusCode.Success,
            Message = message
        };
    }

    /// <summary>
    ///     Создать ответ с ошибкой
    /// </summary>
    public static ApiResponse<T> Error(ApiStatusCode statusCode,
            string message)
    {
        return new ApiResponse<T>
        {
            Data = default,
            StatusCode = statusCode,
            Message = message
        };
    }
}