using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TimeTrackingAPI.Common.Enums;
using TimeTrackingAPI.Data;
using TimeTrackingAPI.Models;

namespace TimeTrackingAPITests.Services.TimeEntryService;

/// <summary>
///     Тесты для метода TimeEntryService.GetTimeEntriesByDateAsync
/// </summary>
[TestClass]
public sealed class
        TimeEntryServiceGetTimeEntriesByDateAsyncTests
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
    ///     Тест: метод возвращает только проводки за указанную дату из смешанных
    ///     данных
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByDateAsync_EntriesFromDifferentDates_ReturnsOnlyForSpecifiedDate()
    {
        // Arrange
        var targetDate = new DateTime(2025, 5, 15);
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

        var targetEntry = new TimeEntry
        {
            Date = targetDate, Hours = 4.0m,
            Description = "Target Work", TaskId = task.Id,
            Task = null!
        };

        var otherEntry1 = new TimeEntry
        {
            Date = targetDate.AddDays(1), Hours = 3.0m,
            Description = "Other Work 1", TaskId = task.Id,
            Task = null!
        };

        var otherEntry2 = new TimeEntry
        {
            Date = targetDate.AddDays(-1), Hours = 5.0m,
            Description = "Other Work 2", TaskId = task.Id,
            Task = null!
        };

        _context.TimeEntries.AddRange(targetEntry, otherEntry1,
                otherEntry2);

        await _context.SaveChangesAsync();

        // Act
        var result =
                await _timeEntryService
                        .GetTimeEntriesByDateAsync(targetDate);

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(1, result.Data.Count);
        Assert.AreEqual(targetDate.Date,
                result.Data[0].Date.Date);
    }

    /// <summary>
    ///     Тест: метод игнорирует компоненты времени при фильтрации
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByDateAsync_DateWithTimeComponents_IgnoresTimeComponents()
    {
        // Arrange
        var baseDate = new DateTime(2025, 5, 15);
        var dateWithTime =
                new DateTime(2025, 5, 15, 14, 30,
                        45); // То же день, но с временем

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

        var morningEntry = new TimeEntry
        {
            Date = baseDate.AddHours(9), Hours = 4.0m,
            Description = "Morning Work", TaskId = task.Id,
            Task = null!
        };

        var eveningEntry = new TimeEntry
        {
            Date = baseDate.AddHours(18), Hours = 3.0m,
            Description = "Evening Work", TaskId = task.Id,
            Task = null!
        };

        _context.TimeEntries.AddRange(morningEntry,
                eveningEntry);

        await _context.SaveChangesAsync();

        // Act - передаем дату с временем
        var result =
                await _timeEntryService
                        .GetTimeEntriesByDateAsync(dateWithTime);

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(2,
                result.Data.Count); // Обе записи за этот день

        Assert.IsTrue(
                result.Data.All(e =>
                        e.Date.Date == baseDate.Date));
    }

    /// <summary>
    ///     Тест: метод возвращает проводки отсортированные по ID по возрастанию
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByDateAsync_MultipleEntriesOnSameDate_ReturnsSortedByIdAscending()
    {
        // Arrange
        var targetDate = new DateTime(2025, 5, 15);
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
            Date = targetDate, Hours = 4.0m,
            Description = "Work 1", TaskId = task.Id,
            Task = null!
        };

        var entry2 = new TimeEntry
        {
            Date = targetDate, Hours = 3.0m,
            Description = "Work 2", TaskId = task.Id,
            Task = null!
        };

        var entry3 = new TimeEntry
        {
            Date = targetDate, Hours = 5.0m,
            Description = "Work 3", TaskId = task.Id,
            Task = null!
        };

        _context.TimeEntries.AddRange(entry1, entry2, entry3);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _timeEntryService
                        .GetTimeEntriesByDateAsync(targetDate);

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(3, result.Data.Count);
        Assert.IsTrue(result.Data[0].Id < result.Data[1].Id);
        Assert.IsTrue(result.Data[1].Id < result.Data[2].Id);
    }

    /// <summary>
    ///     Тест: метод включает связанные задачи и проекты
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByDateAsync_EntriesWithTasksAndProjects_IncludesRelatedData()
    {
        // Arrange
        var targetDate = new DateTime(2025, 5, 15);
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
            Date = targetDate, Hours = 4.0m,
            Description = "Test Work", TaskId = task.Id,
            Task = null!
        };

        _context.TimeEntries.Add(entry);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _timeEntryService
                        .GetTimeEntriesByDateAsync(targetDate);

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(1, result.Data.Count);
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
            GetTimeEntriesByDateAsync_SuccessfulExecution_ReturnsIsSuccessTrue()
    {
        // Arrange
        var targetDate = new DateTime(2025, 5, 15);

        // Act
        var result =
                await _timeEntryService
                        .GetTimeEntriesByDateAsync(targetDate);

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode Success при успешном выполнении
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByDateAsync_SuccessfulExecution_ReturnsStatusCodeSuccess()
    {
        // Arrange
        var targetDate = new DateTime(2025, 5, 15);

        // Act
        var result =
                await _timeEntryService
                        .GetTimeEntriesByDateAsync(targetDate);

        // Assert
        Assert.AreEqual(ApiStatusCode.Success,
                result.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает правильное сообщение с количеством и датой
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByDateAsync_VariousEntryCounts_ReturnsMessageWithCountAndDate()
    {
        // Arrange
        var targetDate = new DateTime(2025, 5, 15);
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
            Date = targetDate, Hours = 4.0m,
            Description = "Work 1", TaskId = task.Id,
            Task = null!
        };

        var entry2 = new TimeEntry
        {
            Date = targetDate, Hours = 3.0m,
            Description = "Work 2", TaskId = task.Id,
            Task = null!
        };

        _context.TimeEntries.AddRange(entry1, entry2);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _timeEntryService
                        .GetTimeEntriesByDateAsync(targetDate);

        // Assert
        Assert.AreEqual("Получено 2 проводок за 2025-05-15",
                result.Message);
    }

    /// <summary>
    ///     Тест: метод логирует запрос с форматированной датой
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByDateAsync_Invoked_LogsInformationWithFormattedDate()
    {
        // Arrange
        var targetDate = new DateTime(2025, 5, 15, 14, 30, 0);

        // Act
        await _timeEntryService.GetTimeEntriesByDateAsync(
                targetDate);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Запрос проводок за дату: 2025-05-15")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод логирует результат с количеством и форматированной датой
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByDateAsync_SuccessfulExecution_LogsInformationWithCountAndDate()
    {
        // Arrange
        var targetDate = new DateTime(2025, 5, 15);
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
            Date = targetDate, Hours = 4.0m,
            Description = "Test Work", TaskId = task.Id,
            Task = null!
        };

        _context.TimeEntries.Add(entry);
        await _context.SaveChangesAsync();

        // Act
        await _timeEntryService.GetTimeEntriesByDateAsync(
                targetDate);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Получено") &&
                                o.ToString()!.Contains('1') &&
                                o.ToString()!.Contains(
                                        "проводок за дату 2025-05-15")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Параметризованный тест: метод возвращает корректное количество проводок за
    ///     дату
    /// </summary>
    [DataTestMethod]
    [DataRow(0, DisplayName = "Нет проводок за дату")]
    [DataRow(1, DisplayName = "Одна проводка за дату")]
    [DataRow(3, DisplayName = "Три проводки за дату")]
    [DataRow(5, DisplayName = "Пять проводок за дату")]
    public async Task
            GetTimeEntriesByDateAsync_VariousEntryCounts_ReturnsCorrectCount(
                    int entryCount)
    {
        // Arrange
        var targetDate = new DateTime(2025, 5, 15);

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
                    Date = targetDate
                            .AddHours(
                                    i), // Разное время, но та же дата
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
                await _timeEntryService
                        .GetTimeEntriesByDateAsync(targetDate);

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(entryCount, result.Data.Count);
        Assert.IsTrue(result.IsSuccess);
        if (entryCount > 0)
            Assert.IsTrue(result.Data.All(e =>
                    e.Date.Date == targetDate.Date));
    }
}