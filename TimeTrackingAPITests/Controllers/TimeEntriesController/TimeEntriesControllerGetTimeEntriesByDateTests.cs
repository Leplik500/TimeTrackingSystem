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
///     Тесты для метода TimeEntriesController.GetTimeEntriesByDate
/// </summary>
[TestClass]
public sealed class
        TimeEntriesControllerGetTimeEntriesByDateTests
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
    ///     Тест: метод вызывает GetTimeEntriesByDateAsync у сервиса с переданной датой
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByDate_WithDate_CallsTimeEntryServiceGetTimeEntriesByDateAsyncWithSameDate()
    {
        // Arrange
        var testDate = new DateTime(2025, 10, 15, 14, 30, 45);
        var serviceResponse =
                ApiResponse<List<TimeEntry>>.Success(
                        [],
                        "Test message");

        _timeEntryService.GetTimeEntriesByDateAsync(testDate)
                .Returns(serviceResponse);

        // Act
        await _controller.GetTimeEntriesByDate(testDate);

        // Assert
        await _timeEntryService.Received(1)
                .GetTimeEntriesByDateAsync(testDate);
    }

    /// <summary>
    ///     Тест: метод возвращает результат сервиса без изменений
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByDate_ServiceReturnsData_ReturnsServiceResponseUnchanged()
    {
        // Arrange
        var testDate = new DateTime(2025, 5, 20);
        var serviceResponse =
                ApiResponse<List<TimeEntry>>.Success(
                        [],
                        "Найдено 3 проводки за дату");

        _timeEntryService.GetTimeEntriesByDateAsync(testDate)
                .Returns(serviceResponse);

        // Act
        var result =
                await _controller.GetTimeEntriesByDate(testDate);

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        var apiResponse =
                objectResult.Value as
                        ApiResponse<List<TimeEntry>>;

        Assert.IsNotNull(apiResponse);

        Assert.AreSame(serviceResponse, apiResponse);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode Success для успешного ответа сервиса
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByDate_ServiceReturnsSuccess_ReturnsStatusCode200()
    {
        // Arrange
        var testDate = new DateTime(2025, 3, 10);
        var serviceResponse =
                ApiResponse<List<TimeEntry>>.Success(
                        [],
                        "Success message");

        _timeEntryService.GetTimeEntriesByDateAsync(testDate)
                .Returns(serviceResponse);

        // Act
        var result =
                await _controller.GetTimeEntriesByDate(testDate);

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
            GetTimeEntriesByDate_ServiceReturnsInternalServerError_ReturnsStatusCode500()
    {
        // Arrange
        var testDate = new DateTime(2025, 7, 25);
        var serviceResponse = ApiResponse<List<TimeEntry>>.Error(
                ApiStatusCode.InternalServerError,
                "Database connection error");

        _timeEntryService.GetTimeEntriesByDateAsync(testDate)
                .Returns(serviceResponse);

        // Act
        var result =
                await _controller.GetTimeEntriesByDate(testDate);

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
            GetTimeEntriesByDate_ServiceResponse_ReturnsActionResultWithApiResponse()
    {
        // Arrange
        var testDate = new DateTime(2025, 12, 1);
        var serviceResponse =
                ApiResponse<List<TimeEntry>>.Success(
                        [],
                        "Test message");

        _timeEntryService.GetTimeEntriesByDateAsync(testDate)
                .Returns(serviceResponse);

        // Act
        var result =
                await _controller.GetTimeEntriesByDate(testDate);

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
    ///     Тест: метод логирует получение запроса с датой в формате yyyy-MM-dd
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByDate_WithDate_LogsInformationWithFormattedDate()
    {
        // Arrange
        var testDate = new DateTime(2025, 8, 30, 16, 45, 20);
        const string expectedFormattedDate = "2025-08-30";
        var serviceResponse =
                ApiResponse<List<TimeEntry>>.Success(
                        [],
                        "Test message");

        _timeEntryService.GetTimeEntriesByDateAsync(testDate)
                .Returns(serviceResponse);

        // Act
        await _controller.GetTimeEntriesByDate(testDate);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Получен запрос на получение проводок за дату:") &&
                                o.ToString()!.Contains(
                                        expectedFormattedDate)),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }
}