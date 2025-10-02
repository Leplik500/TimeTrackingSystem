using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TimeTrackingAPI.Common.Enums;
using TimeTrackingAPI.Data;
using TimeTrackingAPI.Models;

namespace TimeTrackingAPITests.Services.TimeEntryService;

/// <summary>
///     Тесты для метода TimeEntryService.CreateTimeEntryAsync
/// </summary>
[TestClass]
public sealed class TimeEntryServiceCreateTimeEntryAsyncTests
{
    private ApplicationDbContext _context = null!;

    private ILogger<TimeTrackingAPI.Services.TimeEntryService>
            _logger = null!;

    private TimeTrackingAPI.Services.TimeEntryService
            _timeEntryService = null!;

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
        _logger = Substitute
                .For<ILogger<TimeTrackingAPI.Services.TimeEntryService>>();

        _timeEntryService =
                new TimeTrackingAPI.Services.TimeEntryService(
                        _context, _logger);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    /// <summary>
    ///     Тест: метод создает новую проводку времени с корректными данными
    /// </summary>
    [TestMethod]
    public async Task
            CreateTimeEntryAsync_ValidTimeEntry_CreatesEntryWithCorrectData()
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

        var entryDate = new DateTime(2025, 5, 15);
        var timeEntry = new TimeEntry
        {
            Date = entryDate,
            Hours = 4.5m,
            Description = "Test work description",
            TaskId = task.Id,
            Task = null!
        };

        // Act
        var result =
                await _timeEntryService.CreateTimeEntryAsync(
                        timeEntry);

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(entryDate, result.Data.Date);
        Assert.AreEqual(4.5m, result.Data.Hours);
        Assert.AreEqual("Test work description",
                result.Data.Description);

        Assert.AreEqual(task.Id, result.Data.TaskId);
    }

    /// <summary>
    ///     Тест: метод возвращает созданную проводку с сгенерированным ID
    /// </summary>
    [TestMethod]
    public async Task
            CreateTimeEntryAsync_ValidTimeEntry_ReturnsEntryWithGeneratedId()
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
            Hours = 3.0m,
            Description = "Test work",
            TaskId = task.Id,
            Task = null!
        };

        // Act
        var result =
                await _timeEntryService.CreateTimeEntryAsync(
                        timeEntry);

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.IsTrue(result.Data.Id >
                      0); // ID должен быть сгенерирован
    }

    /// <summary>
    ///     Тест: метод загружает связанную задачу после создания
    /// </summary>
    [TestMethod]
    public async Task
            CreateTimeEntryAsync_ValidTimeEntry_LoadsRelatedTask()
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
            Hours = 2.0m,
            Description = "Test work",
            TaskId = task.Id,
            Task = null!
        };

        // Act
        var result =
                await _timeEntryService.CreateTimeEntryAsync(
                        timeEntry);

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.IsNotNull(result.Data.Task);
        Assert.AreEqual("Test Task", result.Data.Task.Name);
        Assert.AreEqual(task.Id, result.Data.Task.Id);
    }

    /// <summary>
    ///     Тест: метод загружает связанный проект через задачу после создания
    /// </summary>
    [TestMethod]
    public async Task
            CreateTimeEntryAsync_ValidTimeEntry_LoadsRelatedProject()
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
            Hours = 5.0m,
            Description = "Test work",
            TaskId = task.Id,
            Task = null!
        };

        // Act
        var result =
                await _timeEntryService.CreateTimeEntryAsync(
                        timeEntry);

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.IsNotNull(result.Data.Task);
        Assert.IsNotNull(result.Data.Task.Project);
        Assert.AreEqual("Test Project",
                result.Data.Task.Project.Name);

        Assert.AreEqual(project.Id, result.Data.Task.Project.Id);
    }

    /// <summary>
    ///     Тест: метод возвращает IsSuccess true при успешном создании
    /// </summary>
    [TestMethod]
    public async Task
            CreateTimeEntryAsync_SuccessfulCreation_ReturnsIsSuccessTrue()
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
            Hours = 1.0m,
            Description = "Test work",
            TaskId = task.Id,
            Task = null!
        };

        // Act
        var result =
                await _timeEntryService.CreateTimeEntryAsync(
                        timeEntry);

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode Success при успешном создании
    /// </summary>
    [TestMethod]
    public async Task
            CreateTimeEntryAsync_SuccessfulCreation_ReturnsStatusCodeSuccess()
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
            Hours = 6.0m,
            Description = "Test work",
            TaskId = task.Id,
            Task = null!
        };

        // Act
        var result =
                await _timeEntryService.CreateTimeEntryAsync(
                        timeEntry);

        // Assert
        Assert.AreEqual(ApiStatusCode.Success,
                result.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает правильное сообщение об успешном создании
    /// </summary>
    [TestMethod]
    public async Task
            CreateTimeEntryAsync_SuccessfulCreation_ReturnsCorrectSuccessMessage()
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
            Hours = 7.0m,
            Description = "Test work",
            TaskId = task.Id,
            Task = null!
        };

        // Act
        var result =
                await _timeEntryService.CreateTimeEntryAsync(
                        timeEntry);

        // Assert
        Assert.AreEqual("Проводка времени успешно создана",
                result.Message);
    }

    /// <summary>
    ///     Тест: метод логирует начало создания с датой и TaskId
    /// </summary>
    [TestMethod]
    public async Task
            CreateTimeEntryAsync_Invoked_LogsInformationWithDateAndTaskId()
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

        var entryDate = new DateTime(2025, 5, 15);
        var timeEntry = new TimeEntry
        {
            Date = entryDate,
            Hours = 8.0m,
            Description = "Test work",
            TaskId = task.Id,
            Task = null!
        };

        // Act
        await _timeEntryService.CreateTimeEntryAsync(timeEntry);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Создание новой проводки времени на дату 2025-05-15") &&
                                o.ToString()!.Contains(
                                        $"для задачи {task.Id}")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод логирует успешное создание с сгенерированным ID
    /// </summary>
    [TestMethod]
    public async Task
            CreateTimeEntryAsync_SuccessfulCreation_LogsInformationWithGeneratedId()
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

        // Act
        var result =
                await _timeEntryService.CreateTimeEntryAsync(
                        timeEntry);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Проводка времени успешно создана с ID:") &&
                                o.ToString()!.Contains(
                                        result.Data!.Id
                                                .ToString())),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }
}