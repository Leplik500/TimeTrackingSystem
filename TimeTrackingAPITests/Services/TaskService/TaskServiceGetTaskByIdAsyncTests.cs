using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TimeTrackingAPI.Common.Enums;
using TimeTrackingAPI.Data;
using TimeTrackingAPI.Models;

namespace TimeTrackingAPITests.Services.TaskService;

/// <summary>
///     Тесты для метода TaskService.GetTaskByIdAsync
/// </summary>
[TestClass]
public sealed class TaskServiceGetTaskByIdAsyncTests
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
    ///     Тест: метод возвращает задачу при существующем ID
    /// </summary>
    [TestMethod]
    public async Task GetTaskByIdAsync_ExistingId_ReturnsTask()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var task = new WorkTask
        {
            Name = "Test Task", ProjectId = 1, IsActive = true,
            Project = project
        };

        _context.Projects.Add(project);
        _context.WorkTasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _taskService.GetTaskByIdAsync(1);

        // Assert
        Assert.IsNotNull(result.Data);
    }

    /// <summary>
    ///     Тест: метод возвращает ошибку NotFound при несуществующем ID
    /// </summary>
    [TestMethod]
    public async Task
            GetTaskByIdAsync_NonExistingId_ReturnsNotFoundError()
    {
        // Arrange
        const int nonExistingId = 999;

        // Act
        var result =
                await _taskService.GetTaskByIdAsync(
                        nonExistingId);

        // Assert
        Assert.IsNull(result.Data);
    }

    /// <summary>
    ///     Тест: метод включает связанный проект в результат
    /// </summary>
    [TestMethod]
    public async Task
            GetTaskByIdAsync_ExistingTaskWithProject_IncludesProjectData()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var task = new WorkTask
        {
            Name = "Test Task", ProjectId = 1, IsActive = true,
            Project = project
        };

        _context.Projects.Add(project);
        _context.WorkTasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _taskService.GetTaskByIdAsync(1);

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.IsNotNull(result.Data.Project);
        Assert.AreEqual("Test Project",
                result.Data.Project.Name);
    }

    /// <summary>
    ///     Тест: метод возвращает IsSuccess true при найденной задаче
    /// </summary>
    [TestMethod]
    public async Task
            GetTaskByIdAsync_ExistingTask_ReturnsIsSuccessTrue()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var task = new WorkTask
        {
            Name = "Test Task", ProjectId = 1, IsActive = true,
            Project = project
        };

        _context.Projects.Add(project);
        _context.WorkTasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _taskService.GetTaskByIdAsync(1);

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode Success при найденной задаче
    /// </summary>
    [TestMethod]
    public async Task
            GetTaskByIdAsync_ExistingTask_ReturnsStatusCodeSuccess()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var task = new WorkTask
        {
            Name = "Test Task", ProjectId = 1, IsActive = true,
            Project = project
        };

        _context.Projects.Add(project);
        _context.WorkTasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _taskService.GetTaskByIdAsync(1);

        // Assert
        Assert.AreEqual(ApiStatusCode.Success,
                result.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает правильное сообщение при найденной задаче
    /// </summary>
    [TestMethod]
    public async Task
            GetTaskByIdAsync_ExistingTask_ReturnsCorrectSuccessMessage()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var task = new WorkTask
        {
            Name = "Test Task", ProjectId = 1, IsActive = true,
            Project = project
        };

        _context.Projects.Add(project);
        _context.WorkTasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _taskService.GetTaskByIdAsync(1);

        // Assert
        Assert.AreEqual("Задача успешно получена",
                result.Message);
    }

    /// <summary>
    ///     Тест: метод возвращает IsSuccess false при ненайденной задаче
    /// </summary>
    [TestMethod]
    public async Task
            GetTaskByIdAsync_NonExistingTask_ReturnsIsSuccessFalse()
    {
        // Arrange
        const int nonExistingId = 999;

        // Act
        var result =
                await _taskService.GetTaskByIdAsync(
                        nonExistingId);

        // Assert
        Assert.IsFalse(result.IsSuccess);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode NotFound при ненайденной задаче
    /// </summary>
    [TestMethod]
    public async Task
            GetTaskByIdAsync_NonExistingTask_ReturnsStatusCodeNotFound()
    {
        // Arrange
        const int nonExistingId = 999;

        // Act
        var result =
                await _taskService.GetTaskByIdAsync(
                        nonExistingId);

        // Assert
        Assert.AreEqual(ApiStatusCode.NotFound,
                result.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает правильное сообщение об ошибке при ненайденной
    ///     задаче
    /// </summary>
    [TestMethod]
    public async Task
            GetTaskByIdAsync_NonExistingTask_ReturnsCorrectErrorMessage()
    {
        // Arrange
        const int nonExistingId = 999;

        // Act
        var result =
                await _taskService.GetTaskByIdAsync(
                        nonExistingId);

        // Assert
        Assert.AreEqual("Задача с ID 999 не найдена",
                result.Message);
    }

    /// <summary>
    ///     Тест: метод возвращает null в Data при ненайденной задаче
    /// </summary>
    [TestMethod]
    public async Task
            GetTaskByIdAsync_NonExistingTask_ReturnsNullData()
    {
        // Arrange
        const int nonExistingId = 999;

        // Act
        var result =
                await _taskService.GetTaskByIdAsync(
                        nonExistingId);

        // Assert
        Assert.IsNull(result.Data);
    }

    /// <summary>
    ///     Тест: метод логирует начало операции с правильным ID
    /// </summary>
    [TestMethod]
    public async Task
            GetTaskByIdAsync_Invoked_LogsInformationWithRequestedId()
    {
        // Arrange
        const int taskId = 123;

        // Act
        await _taskService.GetTaskByIdAsync(taskId);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Запрос на получение задачи с ID: 123")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод логирует успешное нахождение задачи с ID и названием
    /// </summary>
    [TestMethod]
    public async Task
            GetTaskByIdAsync_ExistingTask_LogsInformationWithIdAndName()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var task = new WorkTask
        {
            Name = "Test Task", ProjectId = 1, IsActive = true,
            Project = project
        };

        _context.Projects.Add(project);
        _context.WorkTasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        await _taskService.GetTaskByIdAsync(1);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Задача с ID 1 найдена: Test Task")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод логирует предупреждение при ненайденной задаче
    /// </summary>
    [TestMethod]
    public async Task
            GetTaskByIdAsync_NonExistingTask_LogsWarningWithId()
    {
        // Arrange
        const int nonExistingId = 999;

        // Act
        await _taskService.GetTaskByIdAsync(nonExistingId);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Warning,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Задача с ID 999 не найдена")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Параметризованный тест: метод корректно обрабатывает различные
    ///     несуществующие ID
    /// </summary>
    [DataTestMethod]
    [DataRow(999, DisplayName = "Большой несуществующий ID")]
    [DataRow(0, DisplayName = "ID равен 0")]
    [DataRow(-1, DisplayName = "Отрицательный ID")]
    [DataRow(int.MaxValue,
            DisplayName = "Максимальное значение ID")]
    [DataRow(int.MinValue,
            DisplayName = "Минимальное значение ID")]
    public async Task
            GetTaskByIdAsync_VariousNonExistingIds_ReturnsNotFound(
                    int id)
    {
        // Arrange
        // База данных пуста

        // Act
        var result = await _taskService.GetTaskByIdAsync(id);

        // Assert
        Assert.AreEqual(ApiStatusCode.NotFound,
                result.StatusCode);

        Assert.IsNull(result.Data);
        Assert.IsFalse(result.IsSuccess);
    }
}