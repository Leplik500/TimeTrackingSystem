using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TimeTrackingAPI.Common.Enums;
using TimeTrackingAPI.Data;
using TimeTrackingAPI.Models;

namespace TimeTrackingAPITests.Services.TaskService;

/// <summary>
///     Тесты для метода TaskService.CreateTaskAsync
/// </summary>
[TestClass]
public sealed class TaskServiceCreateTaskAsyncTests
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
    ///     Тест: метод создает задачу при валидных данных и существующем проекте
    /// </summary>
    [TestMethod]
    public async Task
            CreateTaskAsync_ValidTaskWithExistingProject_CreatesTask()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var task = new WorkTask
        {
            Name = "New Task", ProjectId = 1, IsActive = true,
            Project = null!
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        var result = await _taskService.CreateTaskAsync(task);

        // Assert
        Assert.IsNotNull(result.Data);
    }

    /// <summary>
    ///     Тест: метод возвращает ошибку BadRequest при несуществующем проекте
    /// </summary>
    [TestMethod]
    public async Task
            CreateTaskAsync_NonExistingProject_ReturnsBadRequestError()
    {
        // Arrange
        var task = new WorkTask
        {
            Name = "New Task", ProjectId = 999, IsActive = true,
            Project = null!
        };

        // Act
        var result = await _taskService.CreateTaskAsync(task);

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
            CreateTaskAsync_DuplicateTaskNameInProject_ReturnsBadRequestError()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var existingTask = new WorkTask
        {
            Name = "Duplicate Task", ProjectId = 1,
            IsActive = true, Project = project
        };

        var newTask = new WorkTask
        {
            Name = "Duplicate Task", ProjectId = 1,
            IsActive = true, Project = null!
        };

        _context.Projects.Add(project);
        _context.WorkTasks.Add(existingTask);
        await _context.SaveChangesAsync();

        // Act
        var result = await _taskService.CreateTaskAsync(newTask);

        // Assert
        Assert.AreEqual(ApiStatusCode.BadRequest,
                result.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает IsSuccess true при успешном создании
    /// </summary>
    [TestMethod]
    public async Task
            CreateTaskAsync_SuccessfulCreation_ReturnsIsSuccessTrue()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var task = new WorkTask
        {
            Name = "New Task", ProjectId = 1, IsActive = true,
            Project = null!
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        var result = await _taskService.CreateTaskAsync(task);

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode Success при успешном создании
    /// </summary>
    [TestMethod]
    public async Task
            CreateTaskAsync_SuccessfulCreation_ReturnsStatusCodeSuccess()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var task = new WorkTask
        {
            Name = "New Task", ProjectId = 1, IsActive = true,
            Project = null!
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        var result = await _taskService.CreateTaskAsync(task);

        // Assert
        Assert.AreEqual(ApiStatusCode.Success,
                result.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает правильное сообщение при успешном создании
    /// </summary>
    [TestMethod]
    public async Task
            CreateTaskAsync_SuccessfulCreation_ReturnsCorrectSuccessMessage()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var task = new WorkTask
        {
            Name = "New Task", ProjectId = 1, IsActive = true,
            Project = null!
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        var result = await _taskService.CreateTaskAsync(task);

        // Assert
        Assert.AreEqual("Задача успешно создана",
                result.Message);
    }

    /// <summary>
    ///     Тест: метод возвращает правильное сообщение об ошибке при несуществующем
    ///     проекте
    /// </summary>
    [TestMethod]
    public async Task
            CreateTaskAsync_NonExistingProject_ReturnsCorrectErrorMessage()
    {
        // Arrange
        var task = new WorkTask
        {
            Name = "New Task", ProjectId = 999, IsActive = true,
            Project = null!
        };

        // Act
        var result = await _taskService.CreateTaskAsync(task);

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
            CreateTaskAsync_DuplicateTaskName_ReturnsCorrectErrorMessage()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var existingTask = new WorkTask
        {
            Name = "Duplicate Task", ProjectId = 1,
            IsActive = true, Project = project
        };

        var newTask = new WorkTask
        {
            Name = "Duplicate Task", ProjectId = 1,
            IsActive = true, Project = null!
        };

        _context.Projects.Add(project);
        _context.WorkTasks.Add(existingTask);
        await _context.SaveChangesAsync();

        // Act
        var result = await _taskService.CreateTaskAsync(newTask);

        // Assert
        Assert.AreEqual(
                "Задача с названием 'Duplicate Task' уже существует в данном проекте",
                result.Message);
    }

    /// <summary>
    ///     Тест: метод логирует начало создания с названием и ID проекта
    /// </summary>
    [TestMethod]
    public async Task
            CreateTaskAsync_Invoked_LogsInformationWithNameAndProjectId()
    {
        // Arrange
        var task = new WorkTask
        {
            Name = "Test Task", ProjectId = 123, IsActive = true,
            Project = null!
        };

        // Act
        await _taskService.CreateTaskAsync(task);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Создание новой задачи: Test Task для проекта 123")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод логирует предупреждение при несуществующем проекте
    /// </summary>
    [TestMethod]
    public async Task
            CreateTaskAsync_NonExistingProject_LogsWarningWithProjectId()
    {
        // Arrange
        var task = new WorkTask
        {
            Name = "Test Task", ProjectId = 999, IsActive = true,
            Project = null!
        };

        // Act
        await _taskService.CreateTaskAsync(task);

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
            CreateTaskAsync_DuplicateTaskName_LogsWarningWithNameAndProjectId()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var existingTask = new WorkTask
        {
            Name = "Duplicate Task", ProjectId = 1,
            IsActive = true, Project = project
        };

        var newTask = new WorkTask
        {
            Name = "Duplicate Task", ProjectId = 1,
            IsActive = true, Project = null!
        };

        _context.Projects.Add(project);
        _context.WorkTasks.Add(existingTask);
        await _context.SaveChangesAsync();

        // Act
        await _taskService.CreateTaskAsync(newTask);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Warning,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Задача с названием 'Duplicate Task' уже существует в проекте 1")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод логирует успешное создание с ID созданной задачи
    /// </summary>
    [TestMethod]
    public async Task
            CreateTaskAsync_SuccessfulCreation_LogsInformationWithCreatedId()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var task = new WorkTask
        {
            Name = "New Task", ProjectId = 1, IsActive = true,
            Project = null!
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        var result = await _taskService.CreateTaskAsync(task);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        $"Задача успешно создана с ID: {result.Data!.Id}")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод сохраняет задачу в базе данных
    /// </summary>
    [TestMethod]
    public async Task
            CreateTaskAsync_SuccessfulCreation_SavesTaskToDatabase()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var task = new WorkTask
        {
            Name = "New Task", ProjectId = 1, IsActive = true,
            Project = null!
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        await _taskService.CreateTaskAsync(task);

        // Assert
        var savedTask =
                await _context.WorkTasks.FirstOrDefaultAsync(t =>
                        t.Name == "New Task");

        Assert.IsNotNull(savedTask);
        Assert.AreEqual(1, savedTask.ProjectId);
    }

    /// <summary>
    ///     Тест: метод позволяет создать задачи с одинаковыми названиями в разных
    ///     проектах
    /// </summary>
    [TestMethod]
    public async Task
            CreateTaskAsync_SameNameInDifferentProjects_AllowsCreation()
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
            Name = "Same Task", ProjectId = 1, IsActive = true,
            Project = project1
        };

        var task2 = new WorkTask
        {
            Name = "Same Task", ProjectId = 2, IsActive = true,
            Project = null!
        };

        _context.Projects.AddRange(project1, project2);
        _context.WorkTasks.Add(task1);
        await _context.SaveChangesAsync();

        // Act
        var result = await _taskService.CreateTaskAsync(task2);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
    }

    /// <summary>
    ///     Тест: метод загружает связанный проект в созданную задачу
    /// </summary>
    [TestMethod]
    public async Task
            CreateTaskAsync_SuccessfulCreation_LoadsRelatedProject()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var task = new WorkTask
        {
            Name = "New Task", ProjectId = 1, IsActive = true,
            Project = null!
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        var result = await _taskService.CreateTaskAsync(task);

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.IsNotNull(result.Data.Project);
        Assert.AreEqual("Test Project",
                result.Data.Project.Name);
    }
}