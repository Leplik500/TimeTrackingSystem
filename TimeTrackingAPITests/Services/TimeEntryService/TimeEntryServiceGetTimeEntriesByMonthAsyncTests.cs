using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TimeTrackingAPI.Common.Enums;
using TimeTrackingAPI.Data;
using TimeTrackingAPI.Models;

namespace TimeTrackingAPITests.Services.TimeEntryService;

/// <summary>
///     Тесты для метода TimeEntryService.GetTimeEntriesByMonthAsync
/// </summary>
[TestClass]
public sealed class
        TimeEntryServiceGetTimeEntriesByMonthAsyncTests
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
    ///     Тест: метод возвращает только проводки за указанный месяц
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByMonthAsync_EntriesFromDifferentMonths_ReturnsOnlyForSpecifiedMonth()
    {
        // Arrange
        const int targetYear = 2025;
        const int targetMonth = 5;
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
            Date = new DateTime(2025, 5, 15), Hours = 4.0m,
            Description = "Target Work", TaskId = task.Id,
            Task = null!
        };

        var prevMonthEntry = new TimeEntry
        {
            Date = new DateTime(2025, 4, 30), Hours = 3.0m,
            Description = "Previous Month", TaskId = task.Id,
            Task = null!
        };

        var nextMonthEntry = new TimeEntry
        {
            Date = new DateTime(2025, 6, 1), Hours = 5.0m,
            Description = "Next Month", TaskId = task.Id,
            Task = null!
        };

        _context.TimeEntries.AddRange(targetEntry,
                prevMonthEntry, nextMonthEntry);

        await _context.SaveChangesAsync();

        // Act
        var result =
                await _timeEntryService
                        .GetTimeEntriesByMonthAsync(targetYear,
                                targetMonth);

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(1, result.Data.Count);
        Assert.AreEqual(5, result.Data[0].Date.Month);
        Assert.AreEqual(2025, result.Data[0].Date.Year);
    }

    /// <summary>
    ///     Тест: метод включает первый день месяца в диапазон
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByMonthAsync_EntryOnFirstDayOfMonth_IncludesEntry()
    {
        // Arrange
        const int targetYear = 2025;
        const int targetMonth = 5;
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

        var firstDayEntry = new TimeEntry
        {
            Date = new DateTime(2025, 5, 1), Hours = 4.0m,
            Description = "First Day", TaskId = task.Id,
            Task = null!
        };

        _context.TimeEntries.Add(firstDayEntry);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _timeEntryService
                        .GetTimeEntriesByMonthAsync(targetYear,
                                targetMonth);

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(1, result.Data.Count);
        Assert.AreEqual(1, result.Data[0].Date.Day);
    }

    /// <summary>
    ///     Тест: метод исключает первый день следующего месяца из диапазона
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByMonthAsync_EntryOnFirstDayOfNextMonth_ExcludesEntry()
    {
        // Arrange
        const int targetYear = 2025;
        const int targetMonth = 5;
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

        var nextMonthFirstDay = new TimeEntry
        {
            Date = new DateTime(2025, 6, 1), Hours = 4.0m,
            Description = "Next Month First Day",
            TaskId = task.Id, Task = null!
        };

        _context.TimeEntries.Add(nextMonthFirstDay);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _timeEntryService
                        .GetTimeEntriesByMonthAsync(targetYear,
                                targetMonth);

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(0, result.Data.Count);
    }

    /// <summary>
    ///     Тест: метод включает последний день месяца в диапазон
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByMonthAsync_EntryOnLastDayOfMonth_IncludesEntry()
    {
        // Arrange
        const int targetYear = 2025;
        const int targetMonth = 5;
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

        var lastDayEntry = new TimeEntry
        {
            Date = new DateTime(2025, 5, 31), Hours = 4.0m,
            Description = "Last Day", TaskId = task.Id,
            Task = null!
        };

        _context.TimeEntries.Add(lastDayEntry);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _timeEntryService
                        .GetTimeEntriesByMonthAsync(targetYear,
                                targetMonth);

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(1, result.Data.Count);
        Assert.AreEqual(31, result.Data[0].Date.Day);
    }

    /// <summary>
    ///     Тест: метод возвращает проводки отсортированные по дате по убыванию
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByMonthAsync_UnsortedEntriesInMonth_ReturnsSortedByDateDescending()
    {
        // Arrange
        const int targetYear = 2025;
        const int targetMonth = 5;
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

        var earlyEntry = new TimeEntry
        {
            Date = new DateTime(2025, 5, 5), Hours = 4.0m,
            Description = "Early Work", TaskId = task.Id,
            Task = null!
        };

        var lateEntry = new TimeEntry
        {
            Date = new DateTime(2025, 5, 25), Hours = 3.0m,
            Description = "Late Work", TaskId = task.Id,
            Task = null!
        };

        var middleEntry = new TimeEntry
        {
            Date = new DateTime(2025, 5, 15), Hours = 5.0m,
            Description = "Middle Work", TaskId = task.Id,
            Task = null!
        };

        _context.TimeEntries.AddRange(earlyEntry, lateEntry,
                middleEntry);

        await _context.SaveChangesAsync();

        // Act
        var result =
                await _timeEntryService
                        .GetTimeEntriesByMonthAsync(targetYear,
                                targetMonth);

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(3, result.Data.Count);
        Assert.AreEqual(25,
                result.Data[0].Date
                        .Day); // Последняя дата первой

        Assert.AreEqual(15,
                result.Data[1].Date.Day); // Средняя дата второй

        Assert.AreEqual(5,
                result.Data[2].Date
                        .Day); // Ранняя дата последней
    }

    /// <summary>
    ///     Тест: метод сортирует по ID при одинаковых датах
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByMonthAsync_SameDateEntriesInMonth_SortsByIdAscending()
    {
        // Arrange
        const int targetYear = 2025;
        const int targetMonth = 5;
        var sameDate = new DateTime(2025, 5, 15);
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
            Date = sameDate, Hours = 4.0m,
            Description = "Work 1", TaskId = task.Id,
            Task = null!
        };

        var entry2 = new TimeEntry
        {
            Date = sameDate, Hours = 3.0m,
            Description = "Work 2", TaskId = task.Id,
            Task = null!
        };

        var entry3 = new TimeEntry
        {
            Date = sameDate, Hours = 5.0m,
            Description = "Work 3", TaskId = task.Id,
            Task = null!
        };

        _context.TimeEntries.AddRange(entry1, entry2, entry3);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _timeEntryService
                        .GetTimeEntriesByMonthAsync(targetYear,
                                targetMonth);

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
            GetTimeEntriesByMonthAsync_EntriesWithTasksAndProjects_IncludesRelatedData()
    {
        // Arrange
        const int targetYear = 2025;
        const int targetMonth = 5;
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
        var result =
                await _timeEntryService
                        .GetTimeEntriesByMonthAsync(targetYear,
                                targetMonth);

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
            GetTimeEntriesByMonthAsync_SuccessfulExecution_ReturnsIsSuccessTrue()
    {
        // Arrange
        const int targetYear = 2025;
        const int targetMonth = 5;

        // Act
        var result =
                await _timeEntryService
                        .GetTimeEntriesByMonthAsync(targetYear,
                                targetMonth);

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode Success при успешном выполнении
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByMonthAsync_SuccessfulExecution_ReturnsStatusCodeSuccess()
    {
        // Arrange
        const int targetYear = 2025;
        const int targetMonth = 5;

        // Act
        var result =
                await _timeEntryService
                        .GetTimeEntriesByMonthAsync(targetYear,
                                targetMonth);

        // Assert
        Assert.AreEqual(ApiStatusCode.Success,
                result.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает правильное сообщение с количеством и месяцем
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByMonthAsync_VariousEntryCounts_ReturnsMessageWithCountAndMonth()
    {
        // Arrange
        const int targetYear = 2025;
        const int targetMonth = 5;
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
            Description = "Work 1", TaskId = task.Id,
            Task = null!
        };

        var entry2 = new TimeEntry
        {
            Date = new DateTime(2025, 5, 20), Hours = 3.0m,
            Description = "Work 2", TaskId = task.Id,
            Task = null!
        };

        _context.TimeEntries.AddRange(entry1, entry2);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _timeEntryService
                        .GetTimeEntriesByMonthAsync(targetYear,
                                targetMonth);

        // Assert
        Assert.AreEqual("Получено 2 проводок за 2025-05",
                result.Message);
    }

    /// <summary>
    ///     Тест: метод логирует запрос с форматированным годом и месяцем
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByMonthAsync_Invoked_LogsInformationWithFormattedYearMonth()
    {
        // Arrange
        const int targetYear = 2025;
        const int targetMonth = 5;

        // Act
        await _timeEntryService.GetTimeEntriesByMonthAsync(
                targetYear, targetMonth);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Запрос проводок за месяц: 2025-05")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод логирует результат с количеством и форматированным периодом
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByMonthAsync_SuccessfulExecution_LogsInformationWithCountAndYearMonth()
    {
        // Arrange
        const int targetYear = 2025;
        const int targetMonth = 5;
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
        await _timeEntryService.GetTimeEntriesByMonthAsync(
                targetYear, targetMonth);

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
                                        "проводок за 2025-05")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод корректно обрабатывает февраль в високосном году
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByMonthAsync_FebruaryInLeapYear_HandlesCorrectDateRange()
    {
        // Arrange
        const int leapYear = 2024; // 2024 - високосный год
        const int february = 2;
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

        var feb28Entry = new TimeEntry
        {
            Date = new DateTime(2024, 2, 28), Hours = 4.0m,
            Description = "Feb 28", TaskId = task.Id,
            Task = null!
        };

        var feb29Entry = new TimeEntry
        {
            Date = new DateTime(2024, 2, 29), Hours = 3.0m,
            Description = "Feb 29", TaskId = task.Id,
            Task = null!
        };

        var mar1Entry = new TimeEntry
        {
            Date = new DateTime(2024, 3, 1), Hours = 5.0m,
            Description = "Mar 1", TaskId = task.Id, Task = null!
        };

        _context.TimeEntries.AddRange(feb28Entry, feb29Entry,
                mar1Entry);

        await _context.SaveChangesAsync();

        // Act
        var result =
                await _timeEntryService
                        .GetTimeEntriesByMonthAsync(leapYear,
                                february);

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(2,
                result.Data
                        .Count); // Только февральские записи (28 и 29)

        Assert.IsTrue(result.Data.All(e => e.Date is
                {Month: 2, Year: 2024}));
    }

    /// <summary>
    ///     Тест: метод корректно обрабатывает февраль в обычном году
    /// </summary>
    [TestMethod]
    public async Task
            GetTimeEntriesByMonthAsync_FebruaryInNonLeapYear_HandlesCorrectDateRange()
    {
        // Arrange
        const int nonLeapYear = 2025; // 2025 - обычный год
        const int february = 2;
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

        var feb28Entry = new TimeEntry
        {
            Date = new DateTime(2025, 2, 28), Hours = 4.0m,
            Description = "Feb 28", TaskId = task.Id,
            Task = null!
        };

        var mar1Entry = new TimeEntry
        {
            Date = new DateTime(2025, 3, 1), Hours = 5.0m,
            Description = "Mar 1", TaskId = task.Id, Task = null!
        };

        _context.TimeEntries.AddRange(feb28Entry, mar1Entry);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _timeEntryService
                        .GetTimeEntriesByMonthAsync(nonLeapYear,
                                february);

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(1,
                result.Data
                        .Count); // Только одна февральская запись (28)

        Assert.AreEqual(28, result.Data[0].Date.Day);
        Assert.AreEqual(2, result.Data[0].Date.Month);
    }

    /// <summary>
    ///     Параметризованный тест: метод возвращает корректное количество проводок за
    ///     месяц
    /// </summary>
    [DataTestMethod]
    [DataRow(0, DisplayName = "Нет проводок за месяц")]
    [DataRow(1, DisplayName = "Одна проводка за месяц")]
    [DataRow(3, DisplayName = "Три проводки за месяц")]
    [DataRow(10, DisplayName = "Десять проводок за месяц")]
    public async Task
            GetTimeEntriesByMonthAsync_VariousEntryCounts_ReturnsCorrectCount(
                    int entryCount)
    {
        // Arrange
        const int targetYear = 2025;
        const int targetMonth = 5;

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
                var entryDate = new DateTime(2025, 5,
                        Math.Min(i + 1,
                                31)); // Распределяем по дням месяца

                var entry = new TimeEntry
                {
                    Date = entryDate,
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
                        .GetTimeEntriesByMonthAsync(targetYear,
                                targetMonth);

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(entryCount, result.Data.Count);
        Assert.IsTrue(result.IsSuccess);
        if (entryCount > 0)
            Assert.IsTrue(result.Data.All(e => e.Date is
                    {Month: targetMonth, Year: targetYear}));
    }
}