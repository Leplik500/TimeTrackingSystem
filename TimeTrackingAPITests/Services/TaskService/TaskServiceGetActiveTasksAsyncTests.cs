using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TimeTrackingAPI.Common.Enums;
using TimeTrackingAPI.Data;
using TimeTrackingAPI.Models;

namespace TimeTrackingAPITests.Services.TaskService;

/// <summary>
///     Тесты для метода TaskService.GetActiveTasksAsync
/// </summary>
[TestClass]
public sealed class TaskServiceGetActiveTasksAsyncTests
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
                        .UseInMemoryDatabase(
                                Guid.NewGuid()
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
    ///     Тест: метод возвращает только активные задачи из смешанного набора
    /// </summary>
    [TestMethod]
    public async Task
            GetActiveTasksAsync_MixedActiveInactiveTasks_ReturnsOnlyActiveTasks()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var activeTask = new WorkTask
        {
            Name = "Active Task", ProjectId = 1, IsActive = true,
            Project = project
        };

        var inactiveTask = new WorkTask
        {
            Name = "Inactive Task", ProjectId = 1,
            IsActive = false, Project = project
        };

        _context.Projects.Add(project);
        _context.WorkTasks.AddRange(activeTask, inactiveTask);
        await _context.SaveChangesAsync();

        // Act
        var result = await _taskService.GetActiveTasksAsync();

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(1, result.Data.Count);
        Assert.IsTrue(result.Data[0].IsActive);
    }

    /// <summary>
    ///     Тест: метод возвращает активные задачи отсортированные по названию
    /// </summary>
    [TestMethod]
    public async Task
            GetActiveTasksAsync_UnsortedActiveTasks_ReturnsSortedByNameAscending()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        var taskZ = new WorkTask
        {
            Name = "Z Task", ProjectId = 1, IsActive = true,
            Project = project
        };

        var taskA = new WorkTask
        {
            Name = "A Task", ProjectId = 1, IsActive = true,
            Project = project
        };

        var taskM = new WorkTask
        {
            Name = "M Task", ProjectId = 1, IsActive = true,
            Project = project
        };

        _context.Projects.Add(project);
        _context.WorkTasks.AddRange(taskZ, taskA, taskM);
        await _context.SaveChangesAsync();

        // Act
        var result = await _taskService.GetActiveTasksAsync();

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual("A Task", result.Data[0].Name);
        Assert.AreEqual("M Task", result.Data[1].Name);
        Assert.AreEqual("Z Task", result.Data[2].Name);
    }

    /// <summary>
    ///     Тест: метод включает связанные проекты для активных задач
    /// </summary>
    [TestMethod]
    public async Task
            GetActiveTasksAsync_ActiveTasksWithProjects_IncludesRelatedProjects()
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
        var result = await _taskService.GetActiveTasksAsync();

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.IsNotNull(result.Data[0].Project);
        Assert.AreEqual("Test Project",
                result.Data[0].Project.Name);
    }

    /// <summary>
    ///     Тест: метод возвращает IsSuccess true при успешном выполнении
    /// </summary>
    [TestMethod]
    public async Task
            GetActiveTasksAsync_SuccessfulExecution_ReturnsIsSuccessTrue()
    {
        // Arrange
        // База данных может быть пустой

        // Act
        var result = await _taskService.GetActiveTasksAsync();

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode Success при успешном выполнении
    /// </summary>
    [TestMethod]
    public async Task
            GetActiveTasksAsync_SuccessfulExecution_ReturnsStatusCodeSuccess()
    {
        // Arrange
        // База данных может быть пустой

        // Act
        var result = await _taskService.GetActiveTasksAsync();

        // Assert
        Assert.AreEqual(ApiStatusCode.Success,
                result.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает правильное сообщение с количеством активных задач
    /// </summary>
    [TestMethod]
    public async Task
            GetActiveTasksAsync_VariousActiveTaskCounts_ReturnsMessageWithCorrectCount()
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

        _context.Projects.Add(project);
        _context.WorkTasks.AddRange(task1, task2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _taskService.GetActiveTasksAsync();

        // Assert
        Assert.AreEqual("Получено 2 активных рабочих задач",
                result.Message);
    }

    /// <summary>
    ///     Тест: метод логирует начало операции
    /// </summary>
    [TestMethod]
    public async Task
            GetActiveTasksAsync_Invoked_LogsInformationAboutStart()
    {
        // Arrange
        // База данных может быть пустой

        // Act
        await _taskService.GetActiveTasksAsync();

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Запрос на получение только активных рабочих задач")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод логирует успешное завершение с количеством активных задач
    /// </summary>
    [TestMethod]
    public async Task
            GetActiveTasksAsync_SuccessfulExecution_LogsInformationWithCount()
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
        await _taskService.GetActiveTasksAsync();

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
                                        "активных рабочих задач")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод корректно обрабатывает активные задачи из разных проектов
    /// </summary>
    [TestMethod]
    public async Task
            GetActiveTasksAsync_ActiveTasksFromDifferentProjects_ReturnsAllActiveTasks()
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
            Name = "Task 1", ProjectId = 1, IsActive = true,
            Project = project1
        };

        var task2 = new WorkTask
        {
            Name = "Task 2", ProjectId = 2, IsActive = true,
            Project = project2
        };

        _context.Projects.AddRange(project1, project2);
        _context.WorkTasks.AddRange(task1, task2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _taskService.GetActiveTasksAsync();

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(2, result.Data.Count);
        Assert.IsTrue(result.Data.All(t => t.IsActive));
    }

    /// <summary>
    ///     Параметризованный тест: метод возвращает корректное количество активных
    ///     задач
    /// </summary>
    [DataTestMethod]
    [DataRow(0, DisplayName = "Нет активных задач")]
    [DataRow(1, DisplayName = "Одна активная задача")]
    [DataRow(3, DisplayName = "Три активные задачи")]
    [DataRow(5, DisplayName = "Пять активных задач")]
    public async Task
            GetActiveTasksAsync_VariousActiveTaskCounts_ReturnsCorrectCount(
                    int activeTaskCount)
    {
        // Arrange
        if (activeTaskCount > 0)
        {
            var project = new Project
            {
                Name = "Test Project", Code = "TEST01",
                IsActive = true
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            for (var i = 0; i < activeTaskCount; i++)
            {
                var task = new WorkTask
                {
                    Name = $"Task {i + 1}",
                    ProjectId = project.Id,
                    IsActive = true,
                    Project = null!
                };

                _context.WorkTasks.Add(task);
            }

            await _context.SaveChangesAsync();
        }

        // Act
        var result = await _taskService.GetActiveTasksAsync();

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(activeTaskCount, result.Data.Count);
        Assert.IsTrue(result.Data.All(t => t.IsActive));
    }
}