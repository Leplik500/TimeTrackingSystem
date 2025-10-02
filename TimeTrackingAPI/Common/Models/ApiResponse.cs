using TimeTrackingAPI.Common.Enums;

namespace TimeTrackingAPI.Common.Models;

/// <summary>
///     Универсальная модель ответа API
/// </summary>
/// <typeparam name="T">Тип возвращаемых данных</typeparam>
/// <remarks>
///     Используется для стандартизации всех ответов API.
///     Содержит данные, код состояния, сообщение и признак успешности операции.
/// </remarks>
public sealed class ApiResponse<T>
{
    /// <summary>
    ///     Данные ответа
    /// </summary>
    /// <value>Полезная нагрузка ответа или null в случае ошибки</value>
    public T? Data { get; init; }

    /// <summary>
    ///     Код состояния операции
    /// </summary>
    /// <value>Значение из перечисления ApiStatusCode</value>
    public ApiStatusCode StatusCode { get; init; }

    /// <summary>
    ///     Сообщение о результате операции
    /// </summary>
    /// <value>Описание успеха или ошибки</value>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    ///     Признак успешности операции
    /// </summary>
    /// <value>true если StatusCode = Success, иначе false</value>
    /// <remarks>Вычисляемое свойство, зависит от StatusCode</remarks>
    public bool IsSuccess
    {
        get => StatusCode == ApiStatusCode.Success;
    }

    /// <summary>
    ///     Создать успешный ответ API
    /// </summary>
    /// <param name="data">Данные для возврата</param>
    /// <param name="message">
    ///     Сообщение об успехе. По умолчанию "Операция выполнена
    ///     успешно"
    /// </param>
    /// <returns>Объект ApiResponse с кодом Success</returns>
    /// <remarks>Используется для создания положительных ответов с данными</remarks>
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
    /// <param name="statusCode">Код ошибки из перечисления ApiStatusCode</param>
    /// <param name="message">Описание ошибки</param>
    /// <returns>Объект ApiResponse с указанным кодом ошибки и без данных</returns>
    /// <remarks>
    ///     Используется для создания ответов об ошибках. Поле Data
    ///     устанавливается в default
    /// </remarks>
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