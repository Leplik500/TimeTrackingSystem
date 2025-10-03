using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TimeTrackingAPI.Common.DTOs;
using TimeTrackingAPI.Common.Enums;
using TimeTrackingAPI.Common.Models;
using TimeTrackingAPI.Models;
using TimeTrackingAPI.Services.Interfaces;

namespace TimeTrackingAPITests.Controllers.TasksController;

/// <summary>
///     Тесты для метода TasksController.UpdateTask
/// </summary>
[TestClass]
public sealed class TasksControllerUpdateTaskTests
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
    ///     Тест: метод создает WorkTask из параметров с правильными свойствами
    /// </summary>
    [TestMethod]
    public async Task
            UpdateTask_WithIdAndDto_CreatesWorkTaskWithCorrectProperties()
    {
        // Arrange
        const int taskId = 42;
        var taskDto = new TaskUpdateDto
        {
            Name = "Updated Task",
            ProjectId = 15,
            IsActive = false
        };

        WorkTask? capturedTask = null;
        _taskService.UpdateTaskAsync(taskId,
                        Arg.Do<WorkTask>(t => capturedTask = t))
                .Returns(
                        ApiResponse<WorkTask>.Success(null!,
                                "Success"));

        // Act
        await _controller.UpdateTask(taskId, taskDto);

        // Assert
        Assert.IsNotNull(capturedTask);
        Assert.AreEqual(42,
                capturedTask.Id);

        Assert.AreEqual("Updated Task", capturedTask.Name);
        Assert.AreEqual(15, capturedTask.ProjectId);
        Assert.IsFalse(capturedTask.IsActive);
        Assert.IsNull(capturedTask
                .Project);
    }

    /// <summary>
    ///     Тест: метод вызывает UpdateTaskAsync у сервиса с ID и созданной задачей
    /// </summary>
    [TestMethod]
    public async Task
            UpdateTask_WithIdAndDto_CallsTaskServiceUpdateTaskAsyncWithIdAndWorkTask()
    {
        // Arrange
        const int taskId = 123;
        var taskDto = new TaskUpdateDto
        {
            Name = "Service Task",
            ProjectId = 7,
            IsActive = true
        };

        _taskService.UpdateTaskAsync(taskId, Arg.Any<WorkTask>())
                .Returns(
                        ApiResponse<WorkTask>.Success(null!,
                                "Success"));

        // Act
        await _controller.UpdateTask(taskId, taskDto);

        // Assert
        await _taskService.Received(1)
                .UpdateTaskAsync(taskId, Arg.Any<WorkTask>());
    }

    /// <summary>
    ///     Тест: метод возвращает результат сервиса без изменений
    /// </summary>
    [TestMethod]
    public async Task
            UpdateTask_ServiceReturnsData_ReturnsServiceResponseUnchanged()
    {
        // Arrange
        const int taskId = 99;
        var taskDto = new TaskUpdateDto
                {Name = "Test", ProjectId = 1, IsActive = true};

        var serviceResponse =
                ApiResponse<WorkTask>.Success(null!,
                        "Task updated");

        _taskService.UpdateTaskAsync(taskId, Arg.Any<WorkTask>())
                .Returns(serviceResponse);

        // Act
        var result =
                await _controller.UpdateTask(taskId, taskDto);

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        var apiResponse =
                objectResult.Value as ApiResponse<WorkTask>;

        Assert.IsNotNull(apiResponse);

        Assert.AreSame(serviceResponse, apiResponse);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode Success для успешного обновления
    /// </summary>
    [TestMethod]
    public async Task
            UpdateTask_ServiceReturnsSuccess_ReturnsStatusCode200()
    {
        // Arrange
        const int taskId = 50;
        var taskDto = new TaskUpdateDto
        {
            Name = "Success Task", ProjectId = 3, IsActive = true
        };

        var serviceResponse =
                ApiResponse<WorkTask>.Success(null!,
                        "Success message");

        _taskService.UpdateTaskAsync(taskId, Arg.Any<WorkTask>())
                .Returns(serviceResponse);

        // Act
        var result =
                await _controller.UpdateTask(taskId, taskDto);

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
            UpdateTask_ServiceReturnsNotFound_ReturnsStatusCode404()
    {
        // Arrange
        const int taskId = 999;
        var taskDto = new TaskUpdateDto
        {
            Name = "Not Found Task", ProjectId = 5,
            IsActive = true
        };

        var serviceResponse = ApiResponse<WorkTask>.Error(
                ApiStatusCode.NotFound,
                "Task not found");

        _taskService.UpdateTaskAsync(taskId, Arg.Any<WorkTask>())
                .Returns(serviceResponse);

        // Act
        var result =
                await _controller.UpdateTask(taskId, taskDto);

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual((int) HttpStatusCode.NotFound,
                objectResult.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode BadRequest для ошибки валидации
    /// </summary>
    [TestMethod]
    public async Task
            UpdateTask_ServiceReturnsBadRequest_ReturnsStatusCode400()
    {
        // Arrange
        const int taskId = 75;
        var taskDto = new TaskUpdateDto
        {
            Name = "Bad Task", ProjectId = 8, IsActive = true
        };

        var serviceResponse = ApiResponse<WorkTask>.Error(
                ApiStatusCode.BadRequest,
                "Validation failed");

        _taskService.UpdateTaskAsync(taskId, Arg.Any<WorkTask>())
                .Returns(serviceResponse);

        // Act
        var result =
                await _controller.UpdateTask(taskId, taskDto);

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
            UpdateTask_ServiceReturnsInternalServerError_ReturnsStatusCode500()
    {
        // Arrange
        const int taskId = 200;
        var taskDto = new TaskUpdateDto
        {
            Name = "Error Task", ProjectId = 12, IsActive = false
        };

        var serviceResponse = ApiResponse<WorkTask>.Error(
                ApiStatusCode.InternalServerError,
                "Internal error");

        _taskService.UpdateTaskAsync(taskId, Arg.Any<WorkTask>())
                .Returns(serviceResponse);

        // Act
        var result =
                await _controller.UpdateTask(taskId, taskDto);

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
            UpdateTask_ServiceResponse_ReturnsActionResultWithApiResponse()
    {
        // Arrange
        const int taskId = 100;
        var taskDto = new TaskUpdateDto
        {
            Name = "Response Task", ProjectId = 20,
            IsActive = true
        };

        var serviceResponse =
                ApiResponse<WorkTask>.Success(null!, "Success");

        _taskService.UpdateTaskAsync(taskId, Arg.Any<WorkTask>())
                .Returns(serviceResponse);

        // Act
        var result =
                await _controller.UpdateTask(taskId, taskDto);

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
    ///     Тест: метод логирует обновление задачи с ID
    /// </summary>
    [TestMethod]
    public async Task
            UpdateTask_WithId_LogsInformationWithTaskId()
    {
        // Arrange
        const int taskId = 777;
        var taskDto = new TaskUpdateDto
        {
            Name = "Log Test Task",
            ProjectId = 25,
            IsActive = true
        };

        _taskService.UpdateTaskAsync(taskId, Arg.Any<WorkTask>())
                .Returns(
                        ApiResponse<WorkTask>.Success(null!,
                                "Success"));

        // Act
        await _controller.UpdateTask(taskId, taskDto);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Получен запрос на обновление задачи с ID:") &&
                                o.ToString()!.Contains(
                                        taskId.ToString())),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }
}