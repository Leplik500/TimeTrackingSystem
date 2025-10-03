using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TimeTrackingAPI.Common.Enums;
using TimeTrackingAPI.Common.Models;
using TimeTrackingAPI.Models;
using TimeTrackingAPI.Services.Interfaces;

namespace TimeTrackingAPITests.Controllers.TasksController;

/// <summary>
///     Тесты для метода TasksController.GetTasks
/// </summary>
[TestClass]
public sealed class TasksControllerGetTasksTests
{
    private TimeTrackingAPI.Controllers.TasksController
            _controller = null!;

    private ILogger<TimeTrackingAPI.Controllers.TasksController>
            _logger = null!;

    private ITaskService _taskService = null!;

    [TestInitialize]
    public void Setup()
    {
        _taskService = Substitute.For<ITaskService>();
        _logger = Substitute
                .For<ILogger<TimeTrackingAPI.Controllers.TasksController>>();

        _controller =
                new TimeTrackingAPI.Controllers.TasksController(
                        _taskService, _logger);
    }

    /// <summary>
    ///     Тест: метод вызывает GetAllTasksAsync у сервиса
    /// </summary>
    [TestMethod]
    public async Task
            GetTasks_Invoked_CallsTaskServiceGetAllTasksAsync()
    {
        // Arrange
        var serviceResponse =
                ApiResponse<List<WorkTask>>.Success(
                        new List<WorkTask>(),
                        "Test message");

        _taskService.GetAllTasksAsync().Returns(serviceResponse);

        // Act
        await _controller.GetTasks();

        // Assert
        await _taskService.Received(1).GetAllTasksAsync();
    }

    /// <summary>
    ///     Тест: метод возвращает результат сервиса без изменений
    /// </summary>
    [TestMethod]
    public async Task
            GetTasks_ServiceReturnsData_ReturnsServiceResponseUnchanged()
    {
        // Arrange
        var serviceResponse =
                ApiResponse<List<WorkTask>>.Success(
                        [],
                        "Test success message");

        _taskService.GetAllTasksAsync().Returns(serviceResponse);

        // Act
        var result = await _controller.GetTasks();

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        var apiResponse =
                objectResult.Value as
                        ApiResponse<List<WorkTask>>;

        Assert.IsNotNull(apiResponse);

        Assert.AreSame(serviceResponse, apiResponse);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode Success для успешного ответа сервиса
    /// </summary>
    [TestMethod]
    public async Task
            GetTasks_ServiceReturnsSuccess_ReturnsStatusCode200()
    {
        // Arrange
        var serviceResponse =
                ApiResponse<List<WorkTask>>.Success(
                        new List<WorkTask>(),
                        "Success message");

        _taskService.GetAllTasksAsync().Returns(serviceResponse);

        // Act
        var result = await _controller.GetTasks();

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
            GetTasks_ServiceReturnsInternalServerError_ReturnsStatusCode500()
    {
        // Arrange
        var serviceResponse = ApiResponse<List<WorkTask>>.Error(
                ApiStatusCode.InternalServerError,
                "Internal error");

        _taskService.GetAllTasksAsync().Returns(serviceResponse);

        // Act
        var result = await _controller.GetTasks();

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
            GetTasks_ServiceResponse_ReturnsActionResultWithApiResponse()
    {
        // Arrange
        var serviceResponse =
                ApiResponse<List<WorkTask>>.Success(
                        new List<WorkTask>(),
                        "Test message");

        _taskService.GetAllTasksAsync().Returns(serviceResponse);

        // Act
        var result = await _controller.GetTasks();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
        Assert.IsInstanceOfType(result.Result,
                typeof(ObjectResult));

        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.IsInstanceOfType(objectResult.Value,
                typeof(ApiResponse<List<WorkTask>>));
    }

    /// <summary>
    ///     Тест: метод логирует получение запроса на получение всех задач
    /// </summary>
    [TestMethod]
    public async Task
            GetTasks_Invoked_LogsInformationAboutRequest()
    {
        // Arrange
        var serviceResponse =
                ApiResponse<List<WorkTask>>.Success(
                        new List<WorkTask>(),
                        "Test message");

        _taskService.GetAllTasksAsync().Returns(serviceResponse);

        // Act
        await _controller.GetTasks();

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Получен запрос на получение всех задач")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }
}