using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TimeTrackingAPI.Common.Enums;
using TimeTrackingAPI.Common.Models;
using TimeTrackingAPI.Data;
using TimeTrackingAPI.Models;
using TimeTrackingAPI.Services.Interfaces;

namespace TimeTrackingAPITests.Controllers.TimeEntriesController;

/// <summary>
///     Тесты для метода TimeEntriesController.GetTimeEntriesByMonth
/// </summary>
[TestClass]
public sealed class
        TimeEntriesControllerGetTimeEntriesByMonthTests
{
    private ApplicationDbContext _context = null!;

    private TimeTrackingAPI.Controllers.TimeEntriesController
            _controller = null!;

    private
            ILogger<TimeTrackingAPI.Controllers.TimeEntriesController> _logger = null!;

    private ITimeEntryService _timeEntryService = null!;

    [TestInitialize]
    public void Setup()
    {
        var options =
                new DbContextOptionsBuilder<
                                ApplicationDbContext>()
                        .UseInMemoryDatabase(Guid.NewGuid()
                                .ToString())
                        .Options;

        _context = new ApplicationDbContext(options);
        _timeEntryService = Substitute.For<ITimeEntryService>();
        _logger = Substitute
                .For<ILogger<TimeTrackingAPI.Controllers.TimeEntriesController>>();

        _controller =
                new TimeTrackingAPI.Controllers.TimeEntriesController(_context,
                                _timeEntryService, _logger);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Dispose();
    }

    /// <summary>
    ///     Тест: метод возвращает BadRequest для месяца меньше 1
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByMonth_WithMonthLessThan1_ReturnsBadRequest()
    {
        // Arrange
        const int year = 2025;
        const int month = 0;

        // Act
        var result =
                await _controller.GetTimeEntriesByMonth(year,
                        month);

        // Assert
        var badRequestResult =
                result.Result as BadRequestObjectResult;

        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual((int) HttpStatusCode.BadRequest,
                badRequestResult.StatusCode);

        var apiResponse =
                badRequestResult.Value as
                        ApiResponse<List<TimeEntry>>;

        Assert.IsNotNull(apiResponse);
        Assert.AreEqual(ApiStatusCode.BadRequest,
                apiResponse.StatusCode);

        Assert.IsTrue(
                apiResponse.Message.Contains(
                        "Месяц должен быть от 1 до 12"));
    }

    /// <summary>
    ///     Тест: метод возвращает BadRequest для месяца больше 12
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByMonth_WithMonthGreaterThan12_ReturnsBadRequest()
    {
        // Arrange
        const int year = 2025;
        const int month = 13; // Невалидный месяц

        // Act
        var result =
                await _controller.GetTimeEntriesByMonth(year,
                        month);

        // Assert
        var badRequestResult =
                result.Result as BadRequestObjectResult;

        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual((int) HttpStatusCode.BadRequest,
                badRequestResult.StatusCode);

        var apiResponse =
                badRequestResult.Value as
                        ApiResponse<List<TimeEntry>>;

        Assert.IsNotNull(apiResponse);
        Assert.AreEqual(ApiStatusCode.BadRequest,
                apiResponse.StatusCode);

        Assert.IsTrue(
                apiResponse.Message.Contains(
                        "Месяц должен быть от 1 до 12"));
    }

    /// <summary>
    ///     Тест: метод возвращает BadRequest для года меньше 1900
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByMonth_WithYearLessThan1900_ReturnsBadRequest()
    {
        // Arrange
        const int year = 1899; // Невалидный год
        const int month = 5;

        // Act
        var result =
                await _controller.GetTimeEntriesByMonth(year,
                        month);

        // Assert
        var badRequestResult =
                result.Result as BadRequestObjectResult;

        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual((int) HttpStatusCode.BadRequest,
                badRequestResult.StatusCode);

        var apiResponse =
                badRequestResult.Value as
                        ApiResponse<List<TimeEntry>>;

        Assert.IsNotNull(apiResponse);
        Assert.AreEqual(ApiStatusCode.BadRequest,
                apiResponse.StatusCode);

        Assert.IsTrue(
                apiResponse.Message.Contains(
                        "Год должен быть от 1900 до 2100"));
    }

    /// <summary>
    ///     Тест: метод возвращает BadRequest для года больше 2100
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByMonth_WithYearGreaterThan2100_ReturnsBadRequest()
    {
        // Arrange
        const int year = 2101; // Невалидный год
        const int month = 8;

        // Act
        var result =
                await _controller.GetTimeEntriesByMonth(year,
                        month);

        // Assert
        var badRequestResult =
                result.Result as BadRequestObjectResult;

        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual((int) HttpStatusCode.BadRequest,
                badRequestResult.StatusCode);

        var apiResponse =
                badRequestResult.Value as
                        ApiResponse<List<TimeEntry>>;

        Assert.IsNotNull(apiResponse);
        Assert.AreEqual(ApiStatusCode.BadRequest,
                apiResponse.StatusCode);

        Assert.IsTrue(
                apiResponse.Message.Contains(
                        "Год должен быть от 1900 до 2100"));
    }

    /// <summary>
    ///     Тест: метод вызывает GetTimeEntriesByMonthAsync у сервиса с валидными
    ///     параметрами
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByMonth_WithValidParameters_CallsTimeEntryServiceGetTimeEntriesByMonthAsync()
    {
        // Arrange
        const int year = 2025;
        const int month = 6;
        var serviceResponse =
                ApiResponse<List<TimeEntry>>.Success(
                        [],
                        "Test message");

        _timeEntryService.GetTimeEntriesByMonthAsync(year, month)
                .Returns(serviceResponse);

        // Act
        await _controller.GetTimeEntriesByMonth(year, month);

        // Assert
        await _timeEntryService.Received(1)
                .GetTimeEntriesByMonthAsync(year, month);
    }

    /// <summary>
    ///     Тест: метод возвращает результат сервиса без изменений для валидных
    ///     параметров
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByMonth_ServiceReturnsData_ReturnsServiceResponseUnchanged()
    {
        // Arrange
        const int year = 2023;
        const int month = 12;
        var serviceResponse =
                ApiResponse<List<TimeEntry>>.Success(
                        [],
                        "Найдено 15 проводок за месяц");

        _timeEntryService.GetTimeEntriesByMonthAsync(year, month)
                .Returns(serviceResponse);

        // Act
        var result =
                await _controller.GetTimeEntriesByMonth(year,
                        month);

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        var apiResponse =
                objectResult.Value as
                        ApiResponse<List<TimeEntry>>;

        Assert.IsNotNull(apiResponse);

        // Проверяем прозрачность передачи данных контроллером
        Assert.AreSame(serviceResponse, apiResponse);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode Success для успешного ответа сервиса
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByMonth_ServiceReturnsSuccess_ReturnsStatusCode200()
    {
        // Arrange
        const int year = 2022;
        const int month = 3;
        var serviceResponse =
                ApiResponse<List<TimeEntry>>.Success(
                        [],
                        "Success message");

        _timeEntryService.GetTimeEntriesByMonthAsync(year, month)
                .Returns(serviceResponse);

        // Act
        var result =
                await _controller.GetTimeEntriesByMonth(year,
                        month);

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual((int) HttpStatusCode.OK,
                objectResult.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode InternalServerError для ошибки сервиса
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByMonth_ServiceReturnsInternalServerError_ReturnsStatusCode500()
    {
        // Arrange
        const int year = 2021;
        const int month = 9;
        var serviceResponse = ApiResponse<List<TimeEntry>>.Error(
                ApiStatusCode.InternalServerError,
                "Database error");

        _timeEntryService.GetTimeEntriesByMonthAsync(year, month)
                .Returns(serviceResponse);

        // Act
        var result =
                await _controller.GetTimeEntriesByMonth(year,
                        month);

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual((int) HttpStatusCode.InternalServerError,
                objectResult.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает ActionResult с ApiResponse в качестве значения
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByMonth_ServiceResponse_ReturnsActionResultWithApiResponse()
    {
        // Arrange
        const int year = 2020;
        const int month = 11;
        var serviceResponse =
                ApiResponse<List<TimeEntry>>.Success(
                        [],
                        "Test message");

        _timeEntryService.GetTimeEntriesByMonthAsync(year, month)
                .Returns(serviceResponse);

        // Act
        var result =
                await _controller.GetTimeEntriesByMonth(year,
                        month);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
        Assert.IsInstanceOfType(result.Result,
                typeof(ObjectResult));

        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.IsInstanceOfType(objectResult.Value,
                typeof(ApiResponse<List<TimeEntry>>));
    }

    /// <summary>
    ///     Тест: метод логирует получение запроса с годом и месяцем в формате YYYY-MM
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByMonth_WithValidParameters_LogsInformationWithFormattedYearMonth()
    {
        // Arrange
        const int year = 2025;
        const int month = 7;
        const string expectedFormattedDate = "2025-07";
        var serviceResponse =
                ApiResponse<List<TimeEntry>>.Success(
                        [],
                        "Test message");

        _timeEntryService.GetTimeEntriesByMonthAsync(year, month)
                .Returns(serviceResponse);

        // Act
        await _controller.GetTimeEntriesByMonth(year, month);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Получен запрос на получение проводок за месяц:") &&
                                o.ToString()!.Contains(
                                        expectedFormattedDate)),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод не вызывает сервис для невалидного месяца
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByMonth_WithInvalidMonth_DoesNotCallService()
    {
        // Arrange
        const int year = 2025;
        const int month = -5; // Невалидный месяц

        // Act
        await _controller.GetTimeEntriesByMonth(year, month);

        // Assert
        await _timeEntryService.DidNotReceive()
                .GetTimeEntriesByMonthAsync(Arg.Any<int>(),
                        Arg.Any<int>());
    }

    /// <summary>
    ///     Тест: метод не вызывает сервис для невалидного года
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByMonth_WithInvalidYear_DoesNotCallService()
    {
        // Arrange
        const int year = 3000;
        const int month = 4;

        // Act
        await _controller.GetTimeEntriesByMonth(year, month);

        // Assert
        await _timeEntryService.DidNotReceive()
                .GetTimeEntriesByMonthAsync(Arg.Any<int>(),
                        Arg.Any<int>());
    }
}