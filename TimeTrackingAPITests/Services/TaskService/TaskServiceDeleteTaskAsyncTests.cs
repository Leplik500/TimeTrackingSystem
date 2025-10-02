using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TimeTrackingAPI.Common.Enums;
using TimeTrackingAPI.Data;
using TimeTrackingAPI.Models;

namespace TimeTrackingAPITests.Services.TaskService;

/// <summary>
///     Тесты для метода TaskService.DeleteTaskAsync
/// </summary>
[TestClass]
public sealed class TaskServiceDeleteTaskAsyncTests
{
    private ApplicationDbContext _context = null!;

    private ILogger<TimeTrackingAPI.Services.TaskService>
            _logger = null!;

    private TimeTrackingAPI.Services.TaskService _taskService =
            null!;

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
        _logger = Substitute
                .For<ILogger<
                        TimeTrackingAPI.Services.TaskService>>();

        _taskService =
                new TimeTrackingAPI.Services.TaskService(
                        _context, _logger);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    /// <summary>
    ///     Тест: метод удаляет задачу без связанных записей времени
    /// </summary>
    [TestMethod]
    public async Task
            DeleteTaskAsync_TaskWithoutTimeEntries_DeletesTask()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var task = new WorkTask
        {
            Name = "Test Task", ProjectId = project.Id,
            IsActive = true, Project = null!
        };

        _context.WorkTasks.Add(task);
        await _context.SaveChangesAsync();

        // Очищаем tracking контекста
        _context.ChangeTracker.Clear();

        // Act
        var result = await _taskService.DeleteTaskAsync(task.Id);

        // Assert
        Assert.IsTrue(result.Data);
    }

    /// <summary>
    ///     Тест: метод возвращает ошибку NotFound при несуществующем ID задачи
    /// </summary>
    [TestMethod]
    public async Task
            DeleteTaskAsync_NonExistingTask_ReturnsNotFoundError()
    {
        // Arrange
        var nonExistingId = 999;

        // Act
        var result =
                await _taskService
                        .DeleteTaskAsync(nonExistingId);

        // Assert
        Assert.AreEqual(ApiStatusCode.NotFound,
                result.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает ошибку BadRequest при наличии связанных записей
    ///     времени
    /// </summary>
    [TestMethod]
    public async Task
            DeleteTaskAsync_TaskWithTimeEntries_ReturnsBadRequestError()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var task = new WorkTask
        {
            Name = "Test Task", ProjectId = project.Id,
            IsActive = true, Project = null!
        };

        _context.WorkTasks.Add(task);
        await _context.SaveChangesAsync();

        var timeEntry = new TimeEntry
        {
            Date = DateTime.Today,
            Hours = 4.0m,
            Description = "Test work",
            TaskId = task.Id,
            Task = null!
        };

        _context.TimeEntries.Add(timeEntry);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        var result = await _taskService.DeleteTaskAsync(task.Id);

        // Assert
        Assert.AreEqual(ApiStatusCode.BadRequest,
                result.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает IsSuccess true при успешном удалении
    /// </summary>
    [TestMethod]
    public async Task
            DeleteTaskAsync_SuccessfulDeletion_ReturnsIsSuccessTrue()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var task = new WorkTask
        {
            Name = "Test Task", ProjectId = project.Id,
            IsActive = true, Project = null!
        };

        _context.WorkTasks.Add(task);
        await _context.SaveChangesAsync();

        // Очищаем tracking контекста
        _context.ChangeTracker.Clear();

        // Act
        var result = await _taskService.DeleteTaskAsync(task.Id);

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode Success при успешном удалении
    /// </summary>
    [TestMethod]
    public async Task
            DeleteTaskAsync_SuccessfulDeletion_ReturnsStatusCodeSuccess()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var task = new WorkTask
        {
            Name = "Test Task", ProjectId = project.Id,
            IsActive = true, Project = null!
        };

        _context.WorkTasks.Add(task);
        await _context.SaveChangesAsync();

        // Очищаем tracking контекста
        _context.ChangeTracker.Clear();

        // Act
        var result = await _taskService.DeleteTaskAsync(task.Id);

        // Assert
        Assert.AreEqual(ApiStatusCode.Success,
                result.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает правильное сообщение при успешном удалении
    /// </summary>
    [TestMethod]
    public async Task
            DeleteTaskAsync_SuccessfulDeletion_ReturnsCorrectSuccessMessage()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var task = new WorkTask
        {
            Name = "Test Task", ProjectId = project.Id,
            IsActive = true, Project = null!
        };

        _context.WorkTasks.Add(task);
        await _context.SaveChangesAsync();

        // Очищаем tracking контекста
        _context.ChangeTracker.Clear();

        // Act
        var result = await _taskService.DeleteTaskAsync(task.Id);

        // Assert
        Assert.AreEqual("Задача успешно удалена",
                result.Message);
    }

    /// <summary>
    ///     Тест: метод возвращает true в Data при успешном удалении
    /// </summary>
    [TestMethod]
    public async Task
            DeleteTaskAsync_SuccessfulDeletion_ReturnsTrueInData()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var task = new WorkTask
        {
            Name = "Test Task", ProjectId = project.Id,
            IsActive = true, Project = null!
        };

        _context.WorkTasks.Add(task);
        await _context.SaveChangesAsync();

        // Очищаем tracking контекста
        _context.ChangeTracker.Clear();

        // Act
        var result = await _taskService.DeleteTaskAsync(task.Id);

        // Assert
        Assert.IsTrue(result.Data);
    }

    /// <summary>
    ///     Тест: метод возвращает правильное сообщение об ошибке при несуществующей
    ///     задаче
    /// </summary>
    [TestMethod]
    public async Task
            DeleteTaskAsync_NonExistingTask_ReturnsCorrectErrorMessage()
    {
        // Arrange
        var nonExistingId = 999;

        // Act
        var result =
                await _taskService
                        .DeleteTaskAsync(nonExistingId);

        // Assert
        Assert.AreEqual("Задача с ID 999 не найдена",
                result.Message);
    }

    /// <summary>
    ///     Тест: метод возвращает правильное сообщение об ошибке при наличии записей
    ///     времени
    /// </summary>
    [TestMethod]
    public async Task
            DeleteTaskAsync_TaskWithTimeEntries_ReturnsCorrectErrorMessage()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var task = new WorkTask
        {
            Name = "Test Task", ProjectId = project.Id,
            IsActive = true, Project = null!
        };

        _context.WorkTasks.Add(task);
        await _context.SaveChangesAsync();

        var timeEntry1 = new TimeEntry
        {
            Date = DateTime.Today,
            Hours = 4.0m,
            Description = "Work 1",
            TaskId = task.Id,
            Task = null!
        };

        var timeEntry2 = new TimeEntry
        {
            Date = DateTime.Today.AddDays(-1),
            Hours = 3.0m,
            Description = "Work 2",
            TaskId = task.Id,
            Task = null!
        };

        _context.TimeEntries.AddRange(timeEntry1, timeEntry2);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        var result = await _taskService.DeleteTaskAsync(task.Id);

        // Assert
        Assert.AreEqual(
                "Нельзя удалить задачу 'Test Task' - у неё есть 2 связанных записей времени",
                result.Message);
    }

    /// <summary>
    ///     Тест: метод логирует начало удаления с ID задачи
    /// </summary>
    [TestMethod]
    public async Task
            DeleteTaskAsync_Invoked_LogsInformationWithTaskId()
    {
        // Arrange
        var taskId = 123;

        // Act
        await _taskService.DeleteTaskAsync(taskId);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Удаление задачи с ID: 123")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод логирует предупреждение при несуществующей задаче
    /// </summary>
    [TestMethod]
    public async Task
            DeleteTaskAsync_NonExistingTask_LogsWarningWithTaskId()
    {
        // Arrange
        var nonExistingId = 999;

        // Act
        await _taskService.DeleteTaskAsync(nonExistingId);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Warning,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Задача с ID 999 не найдена для удаления")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод логирует предупреждение при наличии связанных записей времени
    /// </summary>
    [TestMethod]
    public async Task
            DeleteTaskAsync_TaskWithTimeEntries_LogsWarningWithTaskIdAndCount()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var task = new WorkTask
        {
            Name = "Test Task", ProjectId = project.Id,
            IsActive = true, Project = null!
        };

        _context.WorkTasks.Add(task);
        await _context.SaveChangesAsync();

        var timeEntry = new TimeEntry
        {
            Date = DateTime.Today,
            Hours = 4.0m,
            Description = "Test work",
            TaskId = task.Id,
            Task = null!
        };

        _context.TimeEntries.Add(timeEntry);
        await _context.SaveChangesAsync();

        // Очищаем tracking контекста
        _context.ChangeTracker.Clear();

        // Act
        await _taskService.DeleteTaskAsync(task.Id);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Warning,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        $"Нельзя удалить задачу с ID {task.Id} - есть связанные записи времени")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод логирует успешное удаление с ID задачи
    /// </summary>
    [TestMethod]
    public async Task
            DeleteTaskAsync_SuccessfulDeletion_LogsInformationWithTaskId()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var task = new WorkTask
        {
            Name = "Test Task", ProjectId = project.Id,
            IsActive = true, Project = null!
        };

        _context.WorkTasks.Add(task);
        await _context.SaveChangesAsync();

        // Очищаем tracking контекста
        _context.ChangeTracker.Clear();

        // Act
        await _taskService.DeleteTaskAsync(task.Id);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        $"Задача с ID {task.Id} успешно удалена")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }
}