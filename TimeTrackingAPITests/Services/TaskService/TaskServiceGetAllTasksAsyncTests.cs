using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TimeTrackingAPI.Common.Enums;
using TimeTrackingAPI.Data;
using TimeTrackingAPI.Models;

namespace TimeTrackingAPITests.Services.TaskService;

/// <summary>
///     Тесты для метода TaskService.GetAllTasksAsync
/// </summary>
[TestClass]
public sealed class TaskServiceGetAllTasksAsyncTests
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
    ///     Тест: метод возвращает пустой список, когда в базе данных нет задач
    /// </summary>
    [TestMethod]
    public async Task
            GetAllTasksAsync_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        // База данных уже пуста

        // Act
        var result = await _taskService.GetAllTasksAsync();

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(0, result.Data.Count);
    }

    /// <summary>
    ///     Тест: метод возвращает одну задачу, когда в базе данных одна задача
    /// </summary>
    [TestMethod]
    public async Task
            GetAllTasksAsync_SingleTask_ReturnsSingleTask()
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
        var result = await _taskService.GetAllTasksAsync();

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(1, result.Data.Count);
    }

    /// <summary>
    ///     Тест: метод возвращает все задачи независимо от статуса активности
    /// </summary>
    [TestMethod]
    public async Task
            GetAllTasksAsync_MixedActiveInactiveTasks_ReturnsAllTasks()
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
        var result = await _taskService.GetAllTasksAsync();

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(2, result.Data.Count);
    }

    /// <summary>
    ///     Тест: метод возвращает задачи отсортированные по названию в алфавитном
    ///     порядке
    /// </summary>
    [TestMethod]
    public async Task
            GetAllTasksAsync_UnsortedTasks_ReturnsSortedByNameAscending()
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
        var result = await _taskService.GetAllTasksAsync();

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual("A Task", result.Data[0].Name);
        Assert.AreEqual("M Task", result.Data[1].Name);
        Assert.AreEqual("Z Task", result.Data[2].Name);
    }

    /// <summary>
    ///     Тест: метод включает связанный проект для каждой задачи
    /// </summary>
    [TestMethod]
    public async Task
            GetAllTasksAsync_TasksWithProjects_IncludesRelatedProjects()
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
        var result = await _taskService.GetAllTasksAsync();

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.IsNotNull(result.Data[0].Project);
        Assert.AreEqual("Test Project",
                result.Data[0].Project.Name);
    }

    /// <summary>
    ///     Тест: метод возвращает IsSuccess равным true при успешном выполнении
    /// </summary>
    [TestMethod]
    public async Task
            GetAllTasksAsync_SuccessfulExecution_ReturnsIsSuccessTrue()
    {
        // Arrange
        // База данных может быть пустой

        // Act
        var result = await _taskService.GetAllTasksAsync();

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode равным Success при успешном выполнении
    /// </summary>
    [TestMethod]
    public async Task
            GetAllTasksAsync_SuccessfulExecution_ReturnsStatusCodeSuccess()
    {
        // Arrange
        // База данных может быть пустой

        // Act
        var result = await _taskService.GetAllTasksAsync();

        // Assert
        Assert.AreEqual(ApiStatusCode.Success,
                result.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает сообщение с корректным количеством задач
    /// </summary>
    [TestMethod]
    public async Task
            GetAllTasksAsync_VariousTaskCounts_ReturnsMessageWithCorrectCount()
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
        var result = await _taskService.GetAllTasksAsync();

        // Assert
        Assert.AreEqual("Получено 2 рабочих задач",
                result.Message);
    }

    /// <summary>
    ///     Тест: метод логирует начало выполнения операции
    /// </summary>
    [TestMethod]
    public async Task
            GetAllTasksAsync_Invoked_LogsInformationAboutStart()
    {
        // Arrange
        // База данных может быть пустой

        // Act
        await _taskService.GetAllTasksAsync();

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Запрос на получение всех рабочих задач")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод логирует успешное завершение с количеством задач
    /// </summary>
    [TestMethod]
    public async Task
            GetAllTasksAsync_SuccessfulExecution_LogsInformationWithCount()
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
        await _taskService.GetAllTasksAsync();

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
                                        "рабочих задач")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод корректно обрабатывает задачи из разных проектов
    /// </summary>
    [TestMethod]
    public async Task
            GetAllTasksAsync_TasksFromDifferentProjects_ReturnsAllTasks()
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
        var result = await _taskService.GetAllTasksAsync();

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(2, result.Data.Count);
    }

    /// <summary>
    ///     Тест: метод корректно обрабатывает задачи с одинаковыми названиями из
    ///     разных проектов
    /// </summary>
    [TestMethod]
    public async Task
            GetAllTasksAsync_TasksWithSameNameInDifferentProjects_ReturnsAllTasks()
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
            Project = project2
        };

        _context.Projects.AddRange(project1, project2);
        _context.WorkTasks.AddRange(task1, task2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _taskService.GetAllTasksAsync();

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(2, result.Data.Count);
        Assert.AreEqual("Same Task", result.Data[0].Name);
        Assert.AreEqual("Same Task", result.Data[1].Name);
    }

    /// <summary>
    ///     Тест: метод возвращает задачи с корректными свойствами проекта
    /// </summary>
    [TestMethod]
    public async Task
            GetAllTasksAsync_TasksWithProjects_ReturnsTasksWithCorrectProjectProperties()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = false
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
        var result = await _taskService.GetAllTasksAsync();

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual("TEST01", result.Data[0].Project.Code);
        Assert.IsFalse(result.Data[0].Project.IsActive);
    }

    /// <summary>
    ///     Параметризованный тест: метод возвращает корректное количество задач
    /// </summary>
    [DataTestMethod]
    [DataRow(0, DisplayName = "Пустая база данных")]
    [DataRow(1, DisplayName = "Одна задача")]
    [DataRow(3, DisplayName = "Три задачи")]
    [DataRow(10, DisplayName = "Десять задач")]
    public async Task
            GetAllTasksAsync_VariousTaskCounts_ReturnsCorrectCount(
                    int taskCount)
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        _context.Projects.Add(project);

        for (var i = 0; i < taskCount; i++)
        {
            var task = new WorkTask
            {
                Name = $"Task {i + 1}",
                ProjectId = 1,
                IsActive = true,
                Project = project
            };

            _context.WorkTasks.Add(task);
        }

        await _context.SaveChangesAsync();

        // Act
        var result = await _taskService.GetAllTasksAsync();

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(taskCount, result.Data.Count);
    }
}