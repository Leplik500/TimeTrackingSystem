using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TimeTrackingAPI.Common.Enums;
using TimeTrackingAPI.Data;
using TimeTrackingAPI.Models;

namespace TimeTrackingAPITests.Services.TaskService;

/// <summary>
///     Тесты для метода TaskService.UpdateTaskAsync
/// </summary>
[TestClass]
public sealed class TaskServiceUpdateTaskAsyncTests
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
    ///     Тест: метод обновляет задачу при валидных данных
    /// </summary>
    [TestMethod]
    public async Task
            UpdateTaskAsync_ValidDataWithExistingTask_UpdatesTask()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var existingTask = new WorkTask
        {
            Name = "Old Name", ProjectId = 1, IsActive = true,
            Project = project
        };

        var updatedTask = new WorkTask
        {
            Name = "New Name", ProjectId = 1, IsActive = false,
            Project = null!
        };

        _context.Projects.Add(project);
        _context.WorkTasks.Add(existingTask);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _taskService.UpdateTaskAsync(1,
                        updatedTask);

        // Assert
        Assert.IsNotNull(result.Data);
    }

    /// <summary>
    ///     Тест: метод возвращает ошибку NotFound при несуществующем ID задачи
    /// </summary>
    [TestMethod]
    public async Task
            UpdateTaskAsync_NonExistingTask_ReturnsNotFoundError()
    {
        // Arrange
        var updatedTask = new WorkTask
        {
            Name = "New Name", ProjectId = 1, IsActive = true,
            Project = null!
        };

        // Act
        var result =
                await _taskService.UpdateTaskAsync(999,
                        updatedTask);

        // Assert
        Assert.AreEqual(ApiStatusCode.NotFound,
                result.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает ошибку BadRequest при несуществующем проекте
    /// </summary>
    [TestMethod]
    public async Task
            UpdateTaskAsync_NonExistingProject_ReturnsBadRequestError()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var existingTask = new WorkTask
        {
            Name = "Task Name", ProjectId = 1, IsActive = true,
            Project = project
        };

        var updatedTask = new WorkTask
        {
            Name = "Updated Name", ProjectId = 999,
            IsActive = true, Project = null!
        };

        _context.Projects.Add(project);
        _context.WorkTasks.Add(existingTask);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _taskService.UpdateTaskAsync(1,
                        updatedTask);

        // Assert
        Assert.AreEqual(ApiStatusCode.BadRequest,
                result.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает ошибку BadRequest при дублировании названия в
    ///     проекте
    /// </summary>
    [TestMethod]
    public async Task
            UpdateTaskAsync_DuplicateTaskNameInProject_ReturnsBadRequestError()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var task1 = new WorkTask
        {
            Name = "Task 1", ProjectId = 1, IsActive = true,
            Project = project
        };

        var task2 = new WorkTask
        {
            Name = "Task 2", ProjectId = 1, IsActive = true,
            Project = project
        };

        var updatedTask = new WorkTask
        {
            Name = "Task 1", ProjectId = 1, IsActive = true,
            Project = null!
        };

        _context.Projects.Add(project);
        _context.WorkTasks.AddRange(task1, task2);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _taskService.UpdateTaskAsync(2,
                        updatedTask);

        // Assert
        Assert.AreEqual(ApiStatusCode.BadRequest,
                result.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает IsSuccess true при успешном обновлении
    /// </summary>
    [TestMethod]
    public async Task
            UpdateTaskAsync_SuccessfulUpdate_ReturnsIsSuccessTrue()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var existingTask = new WorkTask
        {
            Name = "Old Name", ProjectId = 1, IsActive = true,
            Project = project
        };

        var updatedTask = new WorkTask
        {
            Name = "New Name", ProjectId = 1, IsActive = false,
            Project = null!
        };

        _context.Projects.Add(project);
        _context.WorkTasks.Add(existingTask);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _taskService.UpdateTaskAsync(1,
                        updatedTask);

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode Success при успешном обновлении
    /// </summary>
    [TestMethod]
    public async Task
            UpdateTaskAsync_SuccessfulUpdate_ReturnsStatusCodeSuccess()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var existingTask = new WorkTask
        {
            Name = "Old Name", ProjectId = 1, IsActive = true,
            Project = project
        };

        var updatedTask = new WorkTask
        {
            Name = "New Name", ProjectId = 1, IsActive = false,
            Project = null!
        };

        _context.Projects.Add(project);
        _context.WorkTasks.Add(existingTask);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _taskService.UpdateTaskAsync(1,
                        updatedTask);

        // Assert
        Assert.AreEqual(ApiStatusCode.Success,
                result.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает правильное сообщение при успешном обновлении
    /// </summary>
    [TestMethod]
    public async Task
            UpdateTaskAsync_SuccessfulUpdate_ReturnsCorrectSuccessMessage()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var existingTask = new WorkTask
        {
            Name = "Old Name", ProjectId = 1, IsActive = true,
            Project = project
        };

        var updatedTask = new WorkTask
        {
            Name = "New Name", ProjectId = 1, IsActive = false,
            Project = null!
        };

        _context.Projects.Add(project);
        _context.WorkTasks.Add(existingTask);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _taskService.UpdateTaskAsync(1,
                        updatedTask);

        // Assert
        Assert.AreEqual("Задача успешно обновлена",
                result.Message);
    }

    /// <summary>
    ///     Тест: метод возвращает правильное сообщение об ошибке при несуществующей
    ///     задаче
    /// </summary>
    [TestMethod]
    public async Task
            UpdateTaskAsync_NonExistingTask_ReturnsCorrectErrorMessage()
    {
        // Arrange
        var updatedTask = new WorkTask
        {
            Name = "New Name", ProjectId = 1, IsActive = true,
            Project = null!
        };

        // Act
        var result =
                await _taskService.UpdateTaskAsync(999,
                        updatedTask);

        // Assert
        Assert.AreEqual("Задача с ID 999 не найдена",
                result.Message);
    }

    /// <summary>
    ///     Тест: метод возвращает правильное сообщение об ошибке при несуществующем
    ///     проекте
    /// </summary>
    [TestMethod]
    public async Task
            UpdateTaskAsync_NonExistingProject_ReturnsCorrectErrorMessage()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var existingTask = new WorkTask
        {
            Name = "Task Name", ProjectId = 1, IsActive = true,
            Project = project
        };

        var updatedTask = new WorkTask
        {
            Name = "Updated Name", ProjectId = 999,
            IsActive = true, Project = null!
        };

        _context.Projects.Add(project);
        _context.WorkTasks.Add(existingTask);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _taskService.UpdateTaskAsync(1,
                        updatedTask);

        // Assert
        Assert.AreEqual("Проект с ID 999 не существует",
                result.Message);
    }

    /// <summary>
    ///     Тест: метод возвращает правильное сообщение об ошибке при дублировании
    ///     названия
    /// </summary>
    [TestMethod]
    public async Task
            UpdateTaskAsync_DuplicateTaskName_ReturnsCorrectErrorMessage()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var task1 = new WorkTask
        {
            Name = "Task 1", ProjectId = 1, IsActive = true,
            Project = project
        };

        var task2 = new WorkTask
        {
            Name = "Task 2", ProjectId = 1, IsActive = true,
            Project = project
        };

        var updatedTask = new WorkTask
        {
            Name = "Task 1", ProjectId = 1, IsActive = true,
            Project = null!
        };

        _context.Projects.Add(project);
        _context.WorkTasks.AddRange(task1, task2);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _taskService.UpdateTaskAsync(2,
                        updatedTask);

        // Assert
        Assert.AreEqual(
                "Задача с названием 'Task 1' уже существует в данном проекте",
                result.Message);
    }

    /// <summary>
    ///     Тест: метод логирует начало обновления с ID задачи
    /// </summary>
    [TestMethod]
    public async Task
            UpdateTaskAsync_Invoked_LogsInformationWithTaskId()
    {
        // Arrange
        var updatedTask = new WorkTask
        {
            Name = "New Name", ProjectId = 1, IsActive = true,
            Project = null!
        };

        // Act
        await _taskService.UpdateTaskAsync(123, updatedTask);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Обновление задачи с ID: 123")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод логирует предупреждение при несуществующей задаче
    /// </summary>
    [TestMethod]
    public async Task
            UpdateTaskAsync_NonExistingTask_LogsWarningWithTaskId()
    {
        // Arrange
        var updatedTask = new WorkTask
        {
            Name = "New Name", ProjectId = 1, IsActive = true,
            Project = null!
        };

        // Act
        await _taskService.UpdateTaskAsync(999, updatedTask);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Warning,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Задача с ID 999 не найдена для обновления")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод логирует предупреждение при несуществующем проекте
    /// </summary>
    [TestMethod]
    public async Task
            UpdateTaskAsync_NonExistingProject_LogsWarningWithProjectId()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var existingTask = new WorkTask
        {
            Name = "Task Name", ProjectId = 1, IsActive = true,
            Project = project
        };

        var updatedTask = new WorkTask
        {
            Name = "Updated Name", ProjectId = 999,
            IsActive = true, Project = null!
        };

        _context.Projects.Add(project);
        _context.WorkTasks.Add(existingTask);
        await _context.SaveChangesAsync();

        // Act
        await _taskService.UpdateTaskAsync(1, updatedTask);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Warning,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Проект с ID 999 не существует")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод логирует предупреждение при дублировании названия
    /// </summary>
    [TestMethod]
    public async Task
            UpdateTaskAsync_DuplicateTaskName_LogsWarningWithNameAndProjectId()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var task1 = new WorkTask
        {
            Name = "Task 1", ProjectId = 1, IsActive = true,
            Project = project
        };

        var task2 = new WorkTask
        {
            Name = "Task 2", ProjectId = 1, IsActive = true,
            Project = project
        };

        var updatedTask = new WorkTask
        {
            Name = "Task 1", ProjectId = 1, IsActive = true,
            Project = null!
        };

        _context.Projects.Add(project);
        _context.WorkTasks.AddRange(task1, task2);
        await _context.SaveChangesAsync();

        // Act
        await _taskService.UpdateTaskAsync(2, updatedTask);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Warning,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Задача с названием 'Task 1' уже существует в проекте 1")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод логирует успешное обновление с ID задачи
    /// </summary>
    [TestMethod]
    public async Task
            UpdateTaskAsync_SuccessfulUpdate_LogsInformationWithTaskId()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var existingTask = new WorkTask
        {
            Name = "Old Name", ProjectId = 1, IsActive = true,
            Project = project
        };

        var updatedTask = new WorkTask
        {
            Name = "New Name", ProjectId = 1, IsActive = false,
            Project = null!
        };

        _context.Projects.Add(project);
        _context.WorkTasks.Add(existingTask);
        await _context.SaveChangesAsync();

        // Act
        await _taskService.UpdateTaskAsync(1, updatedTask);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Задача с ID 1 успешно обновлена")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод перезагружает связанный проект после обновления
    /// </summary>
    [TestMethod]
    public async Task
            UpdateTaskAsync_SuccessfulUpdate_ReloadsRelatedProject()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var existingTask = new WorkTask
        {
            Name = "Old Name", ProjectId = 1, IsActive = true,
            Project = project
        };

        var updatedTask = new WorkTask
        {
            Name = "New Name", ProjectId = 1, IsActive = false,
            Project = null!
        };

        _context.Projects.Add(project);
        _context.WorkTasks.Add(existingTask);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _taskService.UpdateTaskAsync(1,
                        updatedTask);

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.IsNotNull(result.Data.Project);
        Assert.AreEqual("Test Project",
                result.Data.Project.Name);
    }

    /// <summary>
    ///     Тест: метод позволяет обновить задачу с тем же названием в том же проекте
    /// </summary>
    [TestMethod]
    public async Task
            UpdateTaskAsync_SameNameInSameProject_AllowsUpdate()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var existingTask = new WorkTask
        {
            Name = "Task Name", ProjectId = 1, IsActive = true,
            Project = project
        };

        var updatedTask = new WorkTask
        {
            Name = "Task Name", ProjectId = 1, IsActive = false,
            Project = null!
        };

        _context.Projects.Add(project);
        _context.WorkTasks.Add(existingTask);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _taskService.UpdateTaskAsync(1,
                        updatedTask);

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    /// <summary>
    ///     Тест: метод позволяет обновить задачу с названием, существующим в другом
    ///     проекте
    /// </summary>
    [TestMethod]
    public async Task
            UpdateTaskAsync_SameNameInDifferentProject_AllowsUpdate()
    {
        // Arrange
        var project1 = new Project
        {
            Name = "Project 1", Code = "PROJ01", IsActive = true
        };

        var project2 = new Project
        {
            Name = "Project 2", Code = "PROJ02", IsActive = true
        };

        var task1 = new WorkTask
        {
            Name = "Same Name", ProjectId = 1, IsActive = true,
            Project = project1
        };

        var task2 = new WorkTask
        {
            Name = "Task 2", ProjectId = 2, IsActive = true,
            Project = project2
        };

        var updatedTask = new WorkTask
        {
            Name = "Same Name", ProjectId = 2, IsActive = true,
            Project = null!
        };

        _context.Projects.AddRange(project1, project2);
        _context.WorkTasks.AddRange(task1, task2);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _taskService.UpdateTaskAsync(2,
                        updatedTask);

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    /// <summary>
    ///     Тест: метод запрещает дублирование названия с другой задачей в том же
    ///     проекте
    /// </summary>
    [TestMethod]
    public async Task
            UpdateTaskAsync_DuplicateNameWithAnotherTaskInSameProject_RejectsUpdate()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var task1 = new WorkTask
        {
            Name = "Task 1", ProjectId = 1, IsActive = true,
            Project = project
        };

        var task2 = new WorkTask
        {
            Name = "Task 2", ProjectId = 1, IsActive = true,
            Project = project
        };

        var updatedTask = new WorkTask
        {
            Name = "Task 1", ProjectId = 1, IsActive = true,
            Project = null!
        };

        _context.Projects.Add(project);
        _context.WorkTasks.AddRange(task1, task2);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _taskService.UpdateTaskAsync(2,
                        updatedTask);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(ApiStatusCode.BadRequest,
                result.StatusCode);
    }
}