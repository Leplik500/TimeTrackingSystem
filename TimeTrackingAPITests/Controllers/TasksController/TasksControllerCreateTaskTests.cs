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
///     Тесты для метода TasksController.CreateTask
/// </summary>
[TestClass]
public sealed class TasksControllerCreateTaskTests
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
    ///     Тест: метод создает WorkTask из TaskCreateDto с правильными свойствами
    /// </summary>
    [TestMethod]
    public async Task
            CreateTask_WithTaskDto_CreatesWorkTaskWithCorrectProperties()
    {
        // Arrange
        var taskDto = new TaskCreateDto
        {
            Name = "Test Task",
            ProjectId = 42,
            IsActive = true
        };

        WorkTask? capturedTask = null;
        var createdTask = new WorkTask
        {
            Id = 1,
            Name = "Test Task",
            ProjectId = 42,
            IsActive = true,
            Project = null!
        };

        _taskService
                .CreateTaskAsync(
                        Arg.Do<WorkTask>(t => capturedTask = t))
                .Returns(
                        ApiResponse<WorkTask>.Success(
                                createdTask, "Success"));

        // Act
        await _controller.CreateTask(taskDto);

        // Assert
        Assert.IsNotNull(capturedTask);
        Assert.AreEqual("Test Task", capturedTask.Name);
        Assert.AreEqual(42, capturedTask.ProjectId);
        Assert.IsTrue(capturedTask.IsActive);
        Assert.IsNull(capturedTask.Project);
    }

    /// <summary>
    ///     Тест: метод вызывает CreateTaskAsync у сервиса с созданной задачей
    /// </summary>
    [TestMethod]
    public async Task
            CreateTask_WithTaskDto_CallsTaskServiceCreateTaskAsyncWithWorkTask()
    {
        // Arrange
        var taskDto = new TaskCreateDto
        {
            Name = "Service Task",
            ProjectId = 10,
            IsActive = false
        };

        var createdTask = new WorkTask
        {
            Id = 2,
            Name = "Service Task",
            ProjectId = 10,
            IsActive = false,
            Project = null!
        };

        _taskService.CreateTaskAsync(Arg.Any<WorkTask>())
                .Returns(
                        ApiResponse<WorkTask>.Success(
                                createdTask, "Success"));

        // Act
        await _controller.CreateTask(taskDto);

        // Assert
        await _taskService.Received(1)
                .CreateTaskAsync(Arg.Any<WorkTask>());
    }

    /// <summary>
    ///     Тест: метод возвращает CreatedAtAction для успешного создания
    /// </summary>
    [TestMethod]
    public async Task
            CreateTask_ServiceReturnsSuccess_ReturnsCreatedAtAction()
    {
        // Arrange
        var taskDto = new TaskCreateDto
        {
            Name = "Created Task", ProjectId = 5, IsActive = true
        };

        var createdTask = new WorkTask
        {
            Id = 123,
            Name = "Created Task",
            ProjectId = 5,
            IsActive = true,
            Project = null!
        };

        var serviceResponse =
                ApiResponse<WorkTask>.Success(createdTask,
                        "Task created");

        _taskService.CreateTaskAsync(Arg.Any<WorkTask>())
                .Returns(serviceResponse);

        // Act
        var result = await _controller.CreateTask(taskDto);

        // Assert
        Assert.IsInstanceOfType(result.Result,
                typeof(CreatedAtActionResult));
    }

    /// <summary>
    ///     Тест: метод возвращает правильный route для CreatedAtAction
    /// </summary>
    [TestMethod]
    public async Task
            CreateTask_ServiceReturnsSuccess_ReturnsCreatedAtActionWithCorrectRoute()
    {
        // Arrange
        var taskDto = new TaskCreateDto
        {
            Name = "Route Task", ProjectId = 7, IsActive = true
        };

        var createdTask = new WorkTask
        {
            Id = 456,
            Name = "Route Task",
            ProjectId = 7,
            IsActive = true,
            Project = null!
        };

        var serviceResponse =
                ApiResponse<WorkTask>.Success(createdTask,
                        "Task created");

        _taskService.CreateTaskAsync(Arg.Any<WorkTask>())
                .Returns(serviceResponse);

        // Act
        var result = await _controller.CreateTask(taskDto);

        // Assert
        var createdResult =
                result.Result as CreatedAtActionResult;

        Assert.IsNotNull(createdResult);
        Assert.AreEqual("GetTask", createdResult.ActionName);
        Assert.IsNotNull(createdResult.RouteValues);
        Assert.AreEqual(456, createdResult.RouteValues["id"]);
        Assert.AreSame(serviceResponse, createdResult.Value);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode BadRequest для ошибки валидации
    /// </summary>
    [TestMethod]
    public async Task
            CreateTask_ServiceReturnsBadRequest_ReturnsStatusCode400()
    {
        // Arrange
        var taskDto = new TaskCreateDto
        {
            Name = "Bad Task", ProjectId = 99, IsActive = true
        };

        var serviceResponse = ApiResponse<WorkTask>.Error(
                ApiStatusCode.BadRequest,
                "Validation failed");

        _taskService.CreateTaskAsync(Arg.Any<WorkTask>())
                .Returns(serviceResponse);

        // Act
        var result = await _controller.CreateTask(taskDto);

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
            CreateTask_ServiceReturnsInternalServerError_ReturnsStatusCode500()
    {
        // Arrange
        var taskDto = new TaskCreateDto
        {
            Name = "Error Task", ProjectId = 88, IsActive = true
        };

        var serviceResponse = ApiResponse<WorkTask>.Error(
                ApiStatusCode.InternalServerError,
                "Internal error");

        _taskService.CreateTaskAsync(Arg.Any<WorkTask>())
                .Returns(serviceResponse);

        // Act
        var result = await _controller.CreateTask(taskDto);

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual((int) HttpStatusCode.InternalServerError,
                objectResult.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает ApiResponse в качестве значения для всех случаев
    /// </summary>
    [TestMethod]
    public async Task
            CreateTask_ServiceResponse_ReturnsActionResultWithApiResponse()
    {
        // Arrange
        var taskDto = new TaskCreateDto
        {
            Name = "Response Task", ProjectId = 15,
            IsActive = true
        };

        var createdTask = new WorkTask
        {
            Id = 3,
            Name = "Response Task",
            ProjectId = 15,
            IsActive = true,
            Project = null!
        };

        var serviceResponse =
                ApiResponse<WorkTask>.Success(createdTask,
                        "Success");

        _taskService.CreateTaskAsync(Arg.Any<WorkTask>())
                .Returns(serviceResponse);

        // Act
        var result = await _controller.CreateTask(taskDto);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);

        // Для Success это CreatedAtActionResult
        var createdResult =
                result.Result as CreatedAtActionResult;

        Assert.IsNotNull(createdResult);
        Assert.IsInstanceOfType(createdResult.Value,
                typeof(ApiResponse<WorkTask>));
    }

    /// <summary>
    ///     Тест: метод логирует создание задачи с названием и ID проекта
    /// </summary>
    [TestMethod]
    public async Task
            CreateTask_WithTaskDto_LogsInformationWithNameAndProjectId()
    {
        // Arrange
        var taskDto = new TaskCreateDto
        {
            Name = "Log Test Task",
            ProjectId = 777,
            IsActive = true
        };

        var createdTask = new WorkTask
        {
            Id = 4,
            Name = "Log Test Task",
            ProjectId = 777,
            IsActive = true,
            Project = null!
        };

        _taskService.CreateTaskAsync(Arg.Any<WorkTask>())
                .Returns(
                        ApiResponse<WorkTask>.Success(
                                createdTask, "Success"));

        // Act
        await _controller.CreateTask(taskDto);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Получен запрос на создание задачи:") &&
                                o.ToString()!.Contains(
                                        "Log Test Task") &&
                                o.ToString()!.Contains("777")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }
}