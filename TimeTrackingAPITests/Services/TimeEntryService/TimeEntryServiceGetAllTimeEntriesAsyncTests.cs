using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TimeTrackingAPI.Common.Enums;
using TimeTrackingAPI.Data;
using TimeTrackingAPI.Models;

namespace TimeTrackingAPITests.Services.TimeEntryService;

/// <summary>
///     Тесты для метода TimeEntryService.GetAllTimeEntriesAsync
/// </summary>
[TestClass]
public sealed class TimeEntryServiceGetAllTimeEntriesAsyncTests
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
    ///     Тест: метод возвращает пустой список когда нет проводок времени
    /// </summary>
    [TestMethod]
    public async Task
            GetAllTimeEntriesAsync_NoTimeEntries_ReturnsEmptyList()
    {
        // Arrange
        // База данных пуста

        // Act
        var result =
                await _timeEntryService.GetAllTimeEntriesAsync();

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(0, result.Data.Count);
    }

    /// <summary>
    ///     Тест: метод возвращает все проводки времени
    /// </summary>
    [TestMethod]
    public async Task
            GetAllTimeEntriesAsync_MultipleTimeEntries_ReturnsAllEntries()
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

        var entry1 = new TimeEntry
        {
            Date = DateTime.Today, Hours = 4.0m,
            Description = "Work 1", TaskId = task.Id,
            Task = null!
        };

        var entry2 = new TimeEntry
        {
            Date = DateTime.Today.AddDays(-1), Hours = 3.0m,
            Description = "Work 2", TaskId = task.Id,
            Task = null!
        };

        var entry3 = new TimeEntry
        {
            Date = DateTime.Today.AddDays(-2), Hours = 5.0m,
            Description = "Work 3", TaskId = task.Id,
            Task = null!
        };

        _context.TimeEntries.AddRange(entry1, entry2, entry3);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _timeEntryService.GetAllTimeEntriesAsync();

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(3, result.Data.Count);
    }

    /// <summary>
    ///     Тест: метод возвращает проводки отсортированные по дате по убыванию
    /// </summary>
    [TestMethod]
    public async Task
            GetAllTimeEntriesAsync_UnsortedTimeEntries_ReturnsSortedByDateDescending()
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

        var oldEntry = new TimeEntry
        {
            Date = DateTime.Today.AddDays(-5), Hours = 4.0m,
            Description = "Old Work", TaskId = task.Id,
            Task = null!
        };

        var recentEntry = new TimeEntry
        {
            Date = DateTime.Today, Hours = 3.0m,
            Description = "Recent Work", TaskId = task.Id,
            Task = null!
        };

        var middleEntry = new TimeEntry
        {
            Date = DateTime.Today.AddDays(-2), Hours = 5.0m,
            Description = "Middle Work", TaskId = task.Id,
            Task = null!
        };

        _context.TimeEntries.AddRange(oldEntry, recentEntry,
                middleEntry);

        await _context.SaveChangesAsync();

        // Act
        var result =
                await _timeEntryService.GetAllTimeEntriesAsync();

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(DateTime.Today,
                result.Data[0].Date.Date);

        Assert.AreEqual(DateTime.Today.AddDays(-2),
                result.Data[1].Date.Date);

        Assert.AreEqual(DateTime.Today.AddDays(-5),
                result.Data[2].Date.Date);
    }

    /// <summary>
    ///     Тест: метод сортирует по ID при одинаковых датах
    /// </summary>
    [TestMethod]
    public async Task
            GetAllTimeEntriesAsync_SameDateEntries_SortsByIdAscending()
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

        var entry1 = new TimeEntry
        {
            Date = DateTime.Today, Hours = 4.0m,
            Description = "Work 1", TaskId = task.Id,
            Task = null!
        };

        var entry2 = new TimeEntry
        {
            Date = DateTime.Today, Hours = 3.0m,
            Description = "Work 2", TaskId = task.Id,
            Task = null!
        };

        var entry3 = new TimeEntry
        {
            Date = DateTime.Today, Hours = 5.0m,
            Description = "Work 3", TaskId = task.Id,
            Task = null!
        };

        _context.TimeEntries.AddRange(entry1, entry2, entry3);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _timeEntryService.GetAllTimeEntriesAsync();

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.IsTrue(result.Data[0].Id < result.Data[1].Id);
        Assert.IsTrue(result.Data[1].Id < result.Data[2].Id);
    }

    /// <summary>
    ///     Тест: метод включает связанные задачи и проекты
    /// </summary>
    [TestMethod]
    public async Task
            GetAllTimeEntriesAsync_TimeEntriesWithTasksAndProjects_IncludesRelatedData()
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

        var entry = new TimeEntry
        {
            Date = DateTime.Today, Hours = 4.0m,
            Description = "Test Work", TaskId = task.Id,
            Task = null!
        };

        _context.TimeEntries.Add(entry);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _timeEntryService.GetAllTimeEntriesAsync();

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.IsNotNull(result.Data[0].Task);
        Assert.AreEqual("Test Task", result.Data[0].Task.Name);
        Assert.IsNotNull(result.Data[0].Task.Project);
        Assert.AreEqual("Test Project",
                result.Data[0].Task.Project.Name);
    }

    /// <summary>
    ///     Тест: метод возвращает IsSuccess true при успешном выполнении
    /// </summary>
    [TestMethod]
    public async Task
            GetAllTimeEntriesAsync_SuccessfulExecution_ReturnsIsSuccessTrue()
    {
        // Arrange
        // База данных может быть пустой

        // Act
        var result =
                await _timeEntryService.GetAllTimeEntriesAsync();

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode Success при успешном выполнении
    /// </summary>
    [TestMethod]
    public async Task
            GetAllTimeEntriesAsync_SuccessfulExecution_ReturnsStatusCodeSuccess()
    {
        // Arrange
        // База данных может быть пустой

        // Act
        var result =
                await _timeEntryService.GetAllTimeEntriesAsync();

        // Assert
        Assert.AreEqual(ApiStatusCode.Success,
                result.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает правильное сообщение с количеством проводок
    /// </summary>
    [TestMethod]
    public async Task
            GetAllTimeEntriesAsync_VariousEntryCounts_ReturnsMessageWithCorrectCount()
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

        var entry1 = new TimeEntry
        {
            Date = DateTime.Today, Hours = 4.0m,
            Description = "Work 1", TaskId = task.Id,
            Task = null!
        };

        var entry2 = new TimeEntry
        {
            Date = DateTime.Today.AddDays(-1), Hours = 3.0m,
            Description = "Work 2", TaskId = task.Id,
            Task = null!
        };

        _context.TimeEntries.AddRange(entry1, entry2);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _timeEntryService.GetAllTimeEntriesAsync();

        // Assert
        Assert.AreEqual("Получено 2 проводок времени",
                result.Message);
    }

    /// <summary>
    ///     Тест: метод логирует начало операции
    /// </summary>
    [TestMethod]
    public async Task
            GetAllTimeEntriesAsync_Invoked_LogsInformationAboutStart()
    {
        // Arrange
        // База данных может быть пустой

        // Act
        await _timeEntryService.GetAllTimeEntriesAsync();

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Запрос на получение всех проводок времени")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод логирует успешное завершение с количеством проводок
    /// </summary>
    [TestMethod]
    public async Task
            GetAllTimeEntriesAsync_SuccessfulExecution_LogsInformationWithCount()
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

        var entry = new TimeEntry
        {
            Date = DateTime.Today, Hours = 4.0m,
            Description = "Test Work", TaskId = task.Id,
            Task = null!
        };

        _context.TimeEntries.Add(entry);
        await _context.SaveChangesAsync();

        // Act
        await _timeEntryService.GetAllTimeEntriesAsync();

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Получено") &&
                                o.ToString()!.Contains("1") &&
                                o.ToString()!.Contains(
                                        "проводок времени")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Параметризованный тест: метод возвращает корректное количество проводок
    /// </summary>
    [DataTestMethod]
    [DataRow(0, DisplayName = "Нет проводок времени")]
    [DataRow(1, DisplayName = "Одна проводка времени")]
    [DataRow(3, DisplayName = "Три проводки времени")]
    [DataRow(5, DisplayName = "Пять проводок времени")]
    public async Task
            GetAllTimeEntriesAsync_VariousEntryCounts_ReturnsCorrectCount(
                    int entryCount)
    {
        // Arrange
        if (entryCount > 0)
        {
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

            for (var i = 0; i < entryCount; i++)
            {
                var entry = new TimeEntry
                {
                    Date = DateTime.Today.AddDays(-i),
                    Hours = 4.0m,
                    Description = $"Work {i + 1}",
                    TaskId = task.Id,
                    Task = null!
                };

                _context.TimeEntries.Add(entry);
            }

            await _context.SaveChangesAsync();
        }

        // Act
        var result =
                await _timeEntryService.GetAllTimeEntriesAsync();

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(entryCount, result.Data.Count);
        Assert.IsTrue(result.IsSuccess);
    }
}