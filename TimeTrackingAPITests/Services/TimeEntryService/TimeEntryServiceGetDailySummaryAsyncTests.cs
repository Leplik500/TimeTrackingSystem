using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TimeTrackingAPI.Common.Enums;
using TimeTrackingAPI.Data;
using TimeTrackingAPI.Models;

namespace TimeTrackingAPITests.Services.TimeEntryService;

/// <summary>
///     Тесты для метода TimeEntryService.GetDailySummaryAsync
/// </summary>
[TestClass]
public sealed class TimeEntryServiceGetDailySummaryAsyncTests
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
    ///     Тест: метод группирует проводки по датам и суммирует часы
    /// </summary>
    [TestMethod]
    public async Task
            GetDailySummaryAsync_MultipleEntriesSameDate_GroupsAndSumsHours()
    {
        // Arrange
        var targetDate = new DateTime(2024, 5, 15);
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
            Date = targetDate, Hours = 3.5m,
            Description = "Work 1", TaskId = task.Id,
            Task = null!
        };

        var entry2 = new TimeEntry
        {
            Date = targetDate, Hours = 2.5m,
            Description = "Work 2", TaskId = task.Id,
            Task = null!
        };

        var entry3 = new TimeEntry
        {
            Date = targetDate, Hours = 1.0m,
            Description = "Work 3", TaskId = task.Id,
            Task = null!
        };

        _context.TimeEntries.AddRange(entry1, entry2, entry3);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _timeEntryService.GetDailySummaryAsync();

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(1, result.Data.Count);
        Assert.AreEqual(targetDate.Date, result.Data[0].Date);
        Assert.AreEqual(7.0m,
                result.Data[0].TotalHours); // 3.5 + 2.5 + 1.0
    }

    /// <summary>
    ///     Тест: метод игнорирует время в дате при группировке
    /// </summary>
    [TestMethod]
    public async Task
            GetDailySummaryAsync_EntriesWithDifferentTimes_GroupsByDateOnly()
    {
        // Arrange
        var baseDate = new DateTime(2024, 5, 15);
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

        // Act
        var result =
                await _timeEntryService.GetDailySummaryAsync();

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(1,
                result.Data.Count); // Группировка в один день

        Assert.AreEqual(baseDate.Date, result.Data[0].Date);
        Assert.AreEqual(7.0m,
                result.Data[0].TotalHours); // 4.0 + 3.0
    }

    /// <summary>
    ///     Тест: метод возвращает сводки отсортированные по дате по убыванию
    /// </summary>
    [TestMethod]
    public async Task
            GetDailySummaryAsync_MultipleDates_ReturnsSortedByDateDescending()
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
            Date = new DateTime(2025, 5, 10), Hours = 4.0m,
            Description = "Old Work", TaskId = task.Id,
            Task = null!
        };

        var recentEntry = new TimeEntry
        {
            Date = new DateTime(2025, 5, 20), Hours = 3.0m,
            Description = "Recent Work", TaskId = task.Id,
            Task = null!
        };

        var middleEntry = new TimeEntry
        {
            Date = new DateTime(2025, 5, 15), Hours = 5.0m,
            Description = "Middle Work", TaskId = task.Id,
            Task = null!
        };

        _context.TimeEntries.AddRange(oldEntry, recentEntry,
                middleEntry);

        await _context.SaveChangesAsync();

        // Act
        var result =
                await _timeEntryService.GetDailySummaryAsync();

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(3, result.Data.Count);
        Assert.AreEqual(new DateTime(2025, 5, 20),
                result.Data[0]
                        .Date); // Самая поздняя дата первая

        Assert.AreEqual(new DateTime(2025, 5, 15),
                result.Data[1].Date); // Средняя дата вторая

        Assert.AreEqual(new DateTime(2025, 5, 10),
                result.Data[2].Date); // Ранняя дата последняя
    }

    /// <summary>
    ///     Тест: метод устанавливает статус "insufficient" для менее 8 часов
    /// </summary>
    [TestMethod]
    public async Task
            GetDailySummaryAsync_LessThanEightHours_SetsInsufficientStatus()
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
            Date = targetDate, Hours = 6.5m,
            Description = "Less than 8 hours", TaskId = task.Id,
            Task = null!
        };

        _context.TimeEntries.Add(entry);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _timeEntryService.GetDailySummaryAsync();

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(1, result.Data.Count);
        Assert.AreEqual("insufficient", result.Data[0].Status);
    }

    /// <summary>
    ///     Тест: метод устанавливает статус "sufficient" для ровно 8 часов
    /// </summary>
    [TestMethod]
    public async Task
            GetDailySummaryAsync_ExactlyEightHours_SetsSufficientStatus()
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
            Date = targetDate, Hours = 8.0m,
            Description = "Exactly 8 hours", TaskId = task.Id,
            Task = null!
        };

        _context.TimeEntries.Add(entry);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _timeEntryService.GetDailySummaryAsync();

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(1, result.Data.Count);
        Assert.AreEqual("sufficient", result.Data[0].Status);
    }

    /// <summary>
    ///     Тест: метод устанавливает статус "excessive" для более 8 часов
    /// </summary>
    [TestMethod]
    public async Task
            GetDailySummaryAsync_MoreThanEightHours_SetsExcessiveStatus()
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
            Date = targetDate, Hours = 10.5m,
            Description = "More than 8 hours", TaskId = task.Id,
            Task = null!
        };

        _context.TimeEntries.Add(entry);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _timeEntryService.GetDailySummaryAsync();

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(1, result.Data.Count);
        Assert.AreEqual("excessive", result.Data[0].Status);
    }

    /// <summary>
    ///     Тест: метод возвращает IsSuccess true при успешном выполнении
    /// </summary>
    [TestMethod]
    public async Task
            GetDailySummaryAsync_SuccessfulExecution_ReturnsIsSuccessTrue()
    {
        // Arrange
        // База данных может быть пустой

        // Act
        var result =
                await _timeEntryService.GetDailySummaryAsync();

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode Success при успешном выполнении
    /// </summary>
    [TestMethod]
    public async Task
            GetDailySummaryAsync_SuccessfulExecution_ReturnsStatusCodeSuccess()
    {
        // Arrange
        // База данных может быть пустой

        // Act
        var result =
                await _timeEntryService.GetDailySummaryAsync();

        // Assert
        Assert.AreEqual(ApiStatusCode.Success,
                result.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает правильное сообщение с количеством дней
    /// </summary>
    [TestMethod]
    public async Task
            GetDailySummaryAsync_VariousDayCounts_ReturnsMessageWithCorrectCount()
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
            Date = new DateTime(2025, 5, 10), Hours = 4.0m,
            Description = "Day 1", TaskId = task.Id, Task = null!
        };

        var entry2 = new TimeEntry
        {
            Date = new DateTime(2025, 5, 15), Hours = 3.0m,
            Description = "Day 2", TaskId = task.Id, Task = null!
        };

        _context.TimeEntries.AddRange(entry1, entry2);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _timeEntryService.GetDailySummaryAsync();

        // Assert
        Assert.AreEqual("Получена сводка по 2 дням",
                result.Message);
    }

    /// <summary>
    ///     Тест: метод логирует начало операции
    /// </summary>
    [TestMethod]
    public async Task
            GetDailySummaryAsync_Invoked_LogsInformationAboutStart()
    {
        // Arrange
        // База данных может быть пустой

        // Act
        await _timeEntryService.GetDailySummaryAsync();

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Запрос суммарной информации о часах по дням")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод логирует результат с количеством дней
    /// </summary>
    [TestMethod]
    public async Task
            GetDailySummaryAsync_SuccessfulExecution_LogsInformationWithDayCount()
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
            Date = new DateTime(2025, 5, 15), Hours = 4.0m,
            Description = "Test Work", TaskId = task.Id,
            Task = null!
        };

        _context.TimeEntries.Add(entry);
        await _context.SaveChangesAsync();

        // Act
        await _timeEntryService.GetDailySummaryAsync();

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Получена сводка по") &&
                                o.ToString()!.Contains('1') &&
                                o.ToString()!.Contains("дням")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Параметризованный тест: метод возвращает корректное количество дней в
    ///     сводке
    /// </summary>
    [DataTestMethod]
    [DataRow(0, DisplayName = "Нет дней в сводке")]
    [DataRow(1, DisplayName = "Один день в сводке")]
    [DataRow(3, DisplayName = "Три дня в сводке")]
    [DataRow(7, DisplayName = "Семь дней в сводке")]
    public async Task
            GetDailySummaryAsync_VariousDayCounts_ReturnsCorrectCount(
                    int dayCount)
    {
        // Arrange
        if (dayCount > 0)
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

            for (var i = 0; i < dayCount; i++)
            {
                var entryDate =
                        new DateTime(2025, 5, 1).AddDays(i);

                var entry = new TimeEntry
                {
                    Date = entryDate,
                    Hours = 4.0m,
                    Description = $"Day {i + 1} work",
                    TaskId = task.Id,
                    Task = null!
                };

                _context.TimeEntries.Add(entry);
            }

            await _context.SaveChangesAsync();
        }

        // Act
        var result =
                await _timeEntryService.GetDailySummaryAsync();

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(dayCount, result.Data.Count);
        Assert.IsTrue(result.IsSuccess);
        if (dayCount > 0)
        {
            Assert.IsTrue(
                    result.Data.All(d => d.TotalHours == 4.0m));

            Assert.IsTrue(result.Data.All(d =>
                    d.Status == "insufficient")); // 4 часа < 8
        }
    }
}