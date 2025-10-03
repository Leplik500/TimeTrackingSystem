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
///     Тесты для метода TimeEntriesController.GetTimeEntries
/// </summary>
[TestClass]
public sealed class TimeEntriesControllerGetTimeEntriesTests
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
    ///     Тест: метод вызывает GetAllTimeEntriesAsync у сервиса
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntries_Invoked_CallsTimeEntryServiceGetAllTimeEntriesAsync()
    {
        // Arrange
        var serviceResponse =
                ApiResponse<List<TimeEntry>>.Success(
                        [],
                        "Test message");

        _timeEntryService.GetAllTimeEntriesAsync()
                .Returns(serviceResponse);

        // Act
        await _controller.GetTimeEntries();

        // Assert
        await _timeEntryService.Received(1)
                .GetAllTimeEntriesAsync();
    }

    /// <summary>
    ///     Тест: метод возвращает результат сервиса без изменений
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntries_ServiceReturnsData_ReturnsServiceResponseUnchanged()
    {
        // Arrange
        var serviceResponse =
                ApiResponse<List<TimeEntry>>.Success(
                        [],
                        "Получено 5 проводок времени");

        _timeEntryService.GetAllTimeEntriesAsync()
                .Returns(serviceResponse);

        // Act
        var result = await _controller.GetTimeEntries();

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
            GetTimeEntries_ServiceReturnsSuccess_ReturnsStatusCode200()
    {
        // Arrange
        var serviceResponse =
                ApiResponse<List<TimeEntry>>.Success(
                        [],
                        "Success message");

        _timeEntryService.GetAllTimeEntriesAsync()
                .Returns(serviceResponse);

        // Act
        var result = await _controller.GetTimeEntries();

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
            GetTimeEntries_ServiceReturnsInternalServerError_ReturnsStatusCode500()
    {
        // Arrange
        var serviceResponse = ApiResponse<List<TimeEntry>>.Error(
                ApiStatusCode.InternalServerError,
                "Internal error");

        _timeEntryService.GetAllTimeEntriesAsync()
                .Returns(serviceResponse);

        // Act
        var result = await _controller.GetTimeEntries();

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
            GetTimeEntries_ServiceResponse_ReturnsActionResultWithApiResponse()
    {
        // Arrange
        var serviceResponse =
                ApiResponse<List<TimeEntry>>.Success(
                        [],
                        "Test message");

        _timeEntryService.GetAllTimeEntriesAsync()
                .Returns(serviceResponse);

        // Act
        var result = await _controller.GetTimeEntries();

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
    ///     Тест: метод логирует получение запроса на получение всех проводок времени
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntries_Invoked_LogsInformationAboutRequest()
    {
        // Arrange
        var serviceResponse =
                ApiResponse<List<TimeEntry>>.Success(
                        [],
                        "Test message");

        _timeEntryService.GetAllTimeEntriesAsync()
                .Returns(serviceResponse);

        // Act
        await _controller.GetTimeEntries();

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Получен запрос на получение всех проводок времени")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }
}