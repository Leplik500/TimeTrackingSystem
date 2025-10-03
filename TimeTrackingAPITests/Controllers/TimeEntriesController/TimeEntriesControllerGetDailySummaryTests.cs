using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TimeTrackingAPI.Common.DTOs;
using TimeTrackingAPI.Common.Enums;
using TimeTrackingAPI.Common.Models;
using TimeTrackingAPI.Data;
using TimeTrackingAPI.Services.Interfaces;

namespace TimeTrackingAPITests.Controllers.TimeEntriesController;

/// <summary>
///     Тесты для метода TimeEntriesController.GetDailySummary
/// </summary>
[TestClass]
public sealed class TimeEntriesControllerGetDailySummaryTests
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
                        .UseInMemoryDatabase(
                                Guid.NewGuid()
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
    ///     Тест: метод вызывает GetDailySummaryAsync у сервиса
    /// </summary>
    [TestMethod]
    public async Task
            GetDailySummary_Invoked_CallsTimeEntryServiceGetDailySummaryAsync()
    {
        // Arrange
        var serviceResponse =
                ApiResponse<List<DailyHoursSummaryDto>>.Success(
                        [],
                        "Test message");

        _timeEntryService.GetDailySummaryAsync()
                .Returns(serviceResponse);

        // Act
        await _controller.GetDailySummary();

        // Assert
        await _timeEntryService.Received(1)
                .GetDailySummaryAsync();
    }

    /// <summary>
    ///     Тест: метод возвращает результат сервиса без изменений
    /// </summary>
    [TestMethod]
    public async Task
            GetDailySummary_ServiceReturnsData_ReturnsServiceResponseUnchanged()
    {
        // Arrange
        var serviceResponse =
                ApiResponse<List<DailyHoursSummaryDto>>.Success(
                        [],
                        "Получено 5 дневных сводок");

        _timeEntryService.GetDailySummaryAsync()
                .Returns(serviceResponse);

        // Act
        var result = await _controller.GetDailySummary();

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        var apiResponse =
                objectResult.Value as ApiResponse<
                        List<DailyHoursSummaryDto>>;

        Assert.IsNotNull(apiResponse);

        Assert.AreSame(serviceResponse, apiResponse);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode Success для успешного ответа сервиса
    /// </summary>
    [TestMethod]
    public async Task
            GetDailySummary_ServiceReturnsSuccess_ReturnsStatusCode200()
    {
        // Arrange
        var serviceResponse =
                ApiResponse<List<DailyHoursSummaryDto>>.Success(
                        [],
                        "Success message");

        _timeEntryService.GetDailySummaryAsync()
                .Returns(serviceResponse);

        // Act
        var result = await _controller.GetDailySummary();

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
            GetDailySummary_ServiceReturnsInternalServerError_ReturnsStatusCode500()
    {
        // Arrange
        var serviceResponse =
                ApiResponse<List<DailyHoursSummaryDto>>.Error(
                        ApiStatusCode.InternalServerError,
                        "Database aggregation error");

        _timeEntryService.GetDailySummaryAsync()
                .Returns(serviceResponse);

        // Act
        var result = await _controller.GetDailySummary();

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
            GetDailySummary_ServiceResponse_ReturnsActionResultWithApiResponse()
    {
        // Arrange
        var serviceResponse =
                ApiResponse<List<DailyHoursSummaryDto>>.Success(
                        [],
                        "Test message");

        _timeEntryService.GetDailySummaryAsync()
                .Returns(serviceResponse);

        // Act
        var result = await _controller.GetDailySummary();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
        Assert.IsInstanceOfType(result.Result,
                typeof(ObjectResult));

        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.IsInstanceOfType(objectResult.Value,
                typeof(ApiResponse<List<DailyHoursSummaryDto>>));
    }

    /// <summary>
    ///     Тест: метод логирует получение запроса на дневную сводку
    /// </summary>
    [TestMethod]
    public async Task
            GetDailySummary_Invoked_LogsInformationAboutDailySummaryRequest()
    {
        // Arrange
        var serviceResponse =
                ApiResponse<List<DailyHoursSummaryDto>>.Success(
                        [],
                        "Test message");

        _timeEntryService.GetDailySummaryAsync()
                .Returns(serviceResponse);

        // Act
        await _controller.GetDailySummary();

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Получен запрос на получение дневной сводки часов")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }
}