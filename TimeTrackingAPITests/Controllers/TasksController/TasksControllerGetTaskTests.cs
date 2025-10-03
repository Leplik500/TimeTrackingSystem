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
///     Тесты для метода TasksController.GetTask
/// </summary>
[TestClass]
public sealed class TasksControllerGetTaskTests
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
    ///     Тест: метод вызывает GetTaskByIdAsync у сервиса с переданным ID
    /// </summary>
    [TestMethod]
    public async Task
            GetTask_WithId_CallsTaskServiceGetTaskByIdAsyncWithSameId()
    {
        // Arrange
        const int taskId = 42;
        var serviceResponse =
                ApiResponse<WorkTask>.Success(null!,
                        "Test message");

        _taskService.GetTaskByIdAsync(taskId)
                .Returns(serviceResponse);

        // Act
        await _controller.GetTask(taskId);

        // Assert
        await _taskService.Received(1).GetTaskByIdAsync(taskId);
    }

    /// <summary>
    ///     Тест: метод возвращает результат сервиса без изменений
    /// </summary>
    [TestMethod]
    public async Task
            GetTask_ServiceReturnsData_ReturnsServiceResponseUnchanged()
    {
        // Arrange
        const int taskId = 15;
        var serviceResponse =
                ApiResponse<WorkTask>.Success(null!,
                        "Задача найдена");

        _taskService.GetTaskByIdAsync(taskId)
                .Returns(serviceResponse);

        // Act
        var result = await _controller.GetTask(taskId);

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        var apiResponse =
                objectResult.Value as ApiResponse<WorkTask>;

        Assert.IsNotNull(apiResponse);

        Assert.AreSame(serviceResponse, apiResponse);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode Success для найденной задачи
    /// </summary>
    [TestMethod]
    public async Task
            GetTask_ServiceReturnsSuccess_ReturnsStatusCode200()
    {
        // Arrange
        const int taskId = 100;
        var serviceResponse =
                ApiResponse<WorkTask>.Success(null!,
                        "Success message");

        _taskService.GetTaskByIdAsync(taskId)
                .Returns(serviceResponse);

        // Act
        var result = await _controller.GetTask(taskId);

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual((int) HttpStatusCode.OK,
                objectResult.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode NotFound для не найденной задачи
    /// </summary>
    [TestMethod]
    public async Task
            GetTask_ServiceReturnsNotFound_ReturnsStatusCode404()
    {
        // Arrange
        const int taskId = 999;
        var serviceResponse = ApiResponse<WorkTask>.Error(
                ApiStatusCode.NotFound,
                "Task not found");

        _taskService.GetTaskByIdAsync(taskId)
                .Returns(serviceResponse);

        // Act
        var result = await _controller.GetTask(taskId);

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual((int) HttpStatusCode.NotFound,
                objectResult.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode InternalServerError для ошибки сервиса
    /// </summary>
    [TestMethod]
    public async Task
            GetTask_ServiceReturnsInternalServerError_ReturnsStatusCode500()
    {
        // Arrange
        const int taskId = 50;
        var serviceResponse = ApiResponse<WorkTask>.Error(
                ApiStatusCode.InternalServerError,
                "Internal error");

        _taskService.GetTaskByIdAsync(taskId)
                .Returns(serviceResponse);

        // Act
        var result = await _controller.GetTask(taskId);

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
            GetTask_ServiceResponse_ReturnsActionResultWithApiResponse()
    {
        // Arrange
        const int taskId = 25;
        var serviceResponse =
                ApiResponse<WorkTask>.Success(null!,
                        "Test message");

        _taskService.GetTaskByIdAsync(taskId)
                .Returns(serviceResponse);

        // Act
        var result = await _controller.GetTask(taskId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
        Assert.IsInstanceOfType(result.Result,
                typeof(ObjectResult));

        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.IsInstanceOfType(objectResult.Value,
                typeof(ApiResponse<WorkTask>));
    }

    /// <summary>
    ///     Тест: метод логирует получение запроса с ID задачи
    /// </summary>
    [TestMethod]
    public async Task
            GetTask_InvokedWithId_LogsInformationWithTaskId()
    {
        // Arrange
        const int taskId = 123;
        var serviceResponse =
                ApiResponse<WorkTask>.Success(null!,
                        "Test message");

        _taskService.GetTaskByIdAsync(taskId)
                .Returns(serviceResponse);

        // Act
        await _controller.GetTask(taskId);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Получен запрос на получение задачи с ID:") &&
                                o.ToString()!.Contains(
                                        taskId.ToString())),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }
}