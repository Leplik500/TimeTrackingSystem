using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TimeTrackingAPI.Common.Enums;
using TimeTrackingAPI.Common.Models;
using TimeTrackingAPI.Services.Interfaces;

namespace TimeTrackingAPITests.Controllers.TasksController;

/// <summary>
///     Тесты для метода TasksController.DeleteTask
/// </summary>
[TestClass]
public sealed class TasksControllerDeleteTaskTests
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
    ///     Тест: метод вызывает DeleteTaskAsync у сервиса с переданным ID
    /// </summary>
    [TestMethod]
    public async Task
            DeleteTask_WithId_CallsTaskServiceDeleteTaskAsyncWithSameId()
    {
        // Arrange
        const int taskId = 42;
        var serviceResponse =
                ApiResponse<bool>.Success(true, "Task deleted");

        _taskService.DeleteTaskAsync(taskId)
                .Returns(serviceResponse);

        // Act
        await _controller.DeleteTask(taskId);

        // Assert
        await _taskService.Received(1).DeleteTaskAsync(taskId);
    }

    /// <summary>
    ///     Тест: метод возвращает результат сервиса без изменений
    /// </summary>
    [TestMethod]
    public async Task
            DeleteTask_ServiceReturnsData_ReturnsServiceResponseUnchanged()
    {
        // Arrange
        const int taskId = 15;
        var serviceResponse =
                ApiResponse<bool>.Success(true,
                        "Task deleted successfully");

        _taskService.DeleteTaskAsync(taskId)
                .Returns(serviceResponse);

        // Act
        var result = await _controller.DeleteTask(taskId);

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        var apiResponse =
                objectResult.Value as ApiResponse<bool>;

        Assert.IsNotNull(apiResponse);

        Assert.AreSame(serviceResponse, apiResponse);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode Success для успешного удаления
    /// </summary>
    [TestMethod]
    public async Task
            DeleteTask_ServiceReturnsSuccess_ReturnsStatusCode200()
    {
        // Arrange
        const int taskId = 100;
        var serviceResponse =
                ApiResponse<bool>.Success(true,
                        "Success message");

        _taskService.DeleteTaskAsync(taskId)
                .Returns(serviceResponse);

        // Act
        var result = await _controller.DeleteTask(taskId);

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
            DeleteTask_ServiceReturnsNotFound_ReturnsStatusCode404()
    {
        // Arrange
        const int taskId = 999;
        var serviceResponse = ApiResponse<bool>.Error(
                ApiStatusCode.NotFound,
                "Task not found");

        _taskService.DeleteTaskAsync(taskId)
                .Returns(serviceResponse);

        // Act
        var result = await _controller.DeleteTask(taskId);

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual((int) HttpStatusCode.NotFound,
                objectResult.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode BadRequest для задачи со связанными
    ///     проводками
    /// </summary>
    [TestMethod]
    public async Task
            DeleteTask_ServiceReturnsBadRequest_ReturnsStatusCode400()
    {
        // Arrange
        const int taskId = 50;
        var serviceResponse = ApiResponse<bool>.Error(
                ApiStatusCode.BadRequest,
                "Cannot delete task with related time entries");

        _taskService.DeleteTaskAsync(taskId)
                .Returns(serviceResponse);

        // Act
        var result = await _controller.DeleteTask(taskId);

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual((int) HttpStatusCode.BadRequest,
                objectResult.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode InternalServerError для ошибки сервера
    /// </summary>
    [TestMethod]
    public async Task
            DeleteTask_ServiceReturnsInternalServerError_ReturnsStatusCode500()
    {
        // Arrange
        const int taskId = 75;
        var serviceResponse = ApiResponse<bool>.Error(
                ApiStatusCode.InternalServerError,
                "Internal error");

        _taskService.DeleteTaskAsync(taskId)
                .Returns(serviceResponse);

        // Act
        var result = await _controller.DeleteTask(taskId);

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
            DeleteTask_ServiceResponse_ReturnsActionResultWithApiResponse()
    {
        // Arrange
        const int taskId = 25;
        var serviceResponse =
                ApiResponse<bool>.Success(true, "Test message");

        _taskService.DeleteTaskAsync(taskId)
                .Returns(serviceResponse);

        // Act
        var result = await _controller.DeleteTask(taskId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
        Assert.IsInstanceOfType(result.Result,
                typeof(ObjectResult));

        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.IsInstanceOfType(objectResult.Value,
                typeof(ApiResponse<bool>));
    }

    /// <summary>
    ///     Тест: метод логирует получение запроса с ID задачи
    /// </summary>
    [TestMethod]
    public async Task
            DeleteTask_InvokedWithId_LogsInformationWithTaskId()
    {
        // Arrange
        const int taskId = 123;
        var serviceResponse =
                ApiResponse<bool>.Success(true, "Test message");

        _taskService.DeleteTaskAsync(taskId)
                .Returns(serviceResponse);

        // Act
        await _controller.DeleteTask(taskId);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Получен запрос на удаление задачи с ID:") &&
                                o.ToString()!.Contains(
                                        taskId.ToString())),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }
}