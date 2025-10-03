using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TimeTrackingAPI.Common.Enums;
using TimeTrackingAPI.Data;
using TimeTrackingAPI.Models;

namespace TimeTrackingAPITests.Services.ProjectService;

/// <summary>
///     Тесты для метода ProjectService.DeleteProjectAsync
/// </summary>
[TestClass]
public sealed class ProjectServiceDeleteProjectAsyncTests
{
    private ApplicationDbContext _context = null!;

    private ILogger<TimeTrackingAPI.Services.ProjectService>
            _logger = null!;

    private TimeTrackingAPI.Services.ProjectService
            _projectService = null!;

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
                .For<ILogger<TimeTrackingAPI.Services.ProjectService>>();

        _projectService =
                new TimeTrackingAPI.Services.ProjectService(
                        _context, _logger);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    /// <summary>
    ///     Тест: метод удаляет проект без связанных задач из базы данных
    /// </summary>
    [TestMethod]
    public async Task
            DeleteProjectAsync_ProjectWithoutTasks_DeletesProjectFromDatabase()
    {
        // Arrange
        var project = new Project
        {
            Name = "Project To Delete", Code = "DELETE01",
            IsActive = true
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        var projectId = project.Id;

        _context.ChangeTracker.Clear();

        // Act
        var result =
                await _projectService.DeleteProjectAsync(
                        projectId);

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.IsTrue(result.Data);

        // Проверяем что проект удален из БД
        _context.ChangeTracker.Clear();
        var deletedProject =
                await _context.Projects.FindAsync(projectId);

        Assert.IsNull(deletedProject);
    }

    /// <summary>
    ///     Тест: метод возвращает NotFound для несуществующего проекта
    /// </summary>
    [TestMethod]
    public async Task
            DeleteProjectAsync_NonExistingProject_ReturnsNotFound()
    {
        // Arrange
        const int nonExistingId = 999;

        // Act
        var result =
                await _projectService.DeleteProjectAsync(
                        nonExistingId);

        // Assert
        Assert.IsFalse(result.Data);
    }

    /// <summary>
    ///     Тест: метод возвращает BadRequest для проекта со связанными задачами
    /// </summary>
    [TestMethod]
    public async Task
            DeleteProjectAsync_ProjectWithTasks_ReturnsBadRequest()
    {
        // Arrange
        var project = new Project
        {
            Name = "Project With Tasks", Code = "WITHTASKS",
            IsActive = true
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var task = new WorkTask
        {
            Name = "Related Task", ProjectId = project.Id,
            IsActive = true, Project = null!
        };

        _context.WorkTasks.Add(task);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        var result =
                await _projectService.DeleteProjectAsync(
                        project.Id);

        // Assert
        Assert.IsFalse(result.Data);
    }

    /// <summary>
    ///     Тест: метод возвращает true при успешном удалении
    /// </summary>
    [TestMethod]
    public async Task
            DeleteProjectAsync_SuccessfulDeletion_ReturnsTrue()
    {
        // Arrange
        var project = new Project
        {
            Name = "Success Project", Code = "SUCCESS01",
            IsActive = true
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        var result =
                await _projectService.DeleteProjectAsync(
                        project.Id);

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.IsTrue(result.Data);
    }

    /// <summary>
    ///     Тест: метод возвращает IsSuccess true при успешном удалении
    /// </summary>
    [TestMethod]
    public async Task
            DeleteProjectAsync_SuccessfulDeletion_ReturnsIsSuccessTrue()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        var result =
                await _projectService.DeleteProjectAsync(
                        project.Id);

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    /// <summary>
    ///     Тест: метод возвращает IsSuccess false для несуществующего проекта
    /// </summary>
    [TestMethod]
    public async Task
            DeleteProjectAsync_NonExistingProject_ReturnsIsSuccessFalse()
    {
        // Arrange
        const int nonExistingId = 999;

        // Act
        var result =
                await _projectService.DeleteProjectAsync(
                        nonExistingId);

        // Assert
        Assert.IsFalse(result.IsSuccess);
    }

    /// <summary>
    ///     Тест: метод возвращает IsSuccess false для проекта со связанными задачами
    /// </summary>
    [TestMethod]
    public async Task
            DeleteProjectAsync_ProjectWithTasks_ReturnsIsSuccessFalse()
    {
        // Arrange
        var project = new Project
        {
            Name = "Project With Tasks", Code = "TASKS01",
            IsActive = true
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var task = new WorkTask
        {
            Name = "Related Task", ProjectId = project.Id,
            IsActive = true, Project = null!
        };

        _context.WorkTasks.Add(task);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        var result =
                await _projectService.DeleteProjectAsync(
                        project.Id);

        // Assert
        Assert.IsFalse(result.IsSuccess);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode Success при успешном удалении
    /// </summary>
    [TestMethod]
    public async Task
            DeleteProjectAsync_SuccessfulDeletion_ReturnsStatusCodeSuccess()
    {
        // Arrange
        var project = new Project
        {
            Name = "Status Project", Code = "STATUS01",
            IsActive = true
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        var result =
                await _projectService.DeleteProjectAsync(
                        project.Id);

        // Assert
        Assert.AreEqual(ApiStatusCode.Success,
                result.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode NotFound для несуществующего проекта
    /// </summary>
    [TestMethod]
    public async Task
            DeleteProjectAsync_NonExistingProject_ReturnsStatusCodeNotFound()
    {
        // Arrange
        const int nonExistingId = 999;

        // Act
        var result =
                await _projectService.DeleteProjectAsync(
                        nonExistingId);

        // Assert
        Assert.AreEqual(ApiStatusCode.NotFound,
                result.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode BadRequest для проекта со связанными
    ///     задачами
    /// </summary>
    [TestMethod]
    public async Task
            DeleteProjectAsync_ProjectWithTasks_ReturnsStatusCodeBadRequest()
    {
        // Arrange
        var project = new Project
        {
            Name = "BadRequest Project", Code = "BADREQ01",
            IsActive = true
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var task = new WorkTask
        {
            Name = "Related Task", ProjectId = project.Id,
            IsActive = true, Project = null!
        };

        _context.WorkTasks.Add(task);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        var result =
                await _projectService.DeleteProjectAsync(
                        project.Id);

        // Assert
        Assert.AreEqual(ApiStatusCode.BadRequest,
                result.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает правильное сообщение об успехе
    /// </summary>
    [TestMethod]
    public async Task
            DeleteProjectAsync_SuccessfulDeletion_ReturnsCorrectSuccessMessage()
    {
        // Arrange
        var project = new Project
        {
            Name = "Message Project", Code = "MSG01",
            IsActive = true
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        var result =
                await _projectService.DeleteProjectAsync(
                        project.Id);

        // Assert
        Assert.AreEqual("Проект успешно удален", result.Message);
    }

    /// <summary>
    ///     Тест: метод возвращает правильное сообщение NotFound с ID
    /// </summary>
    [TestMethod]
    public async Task
            DeleteProjectAsync_NonExistingProject_ReturnsCorrectNotFoundMessageWithId()
    {
        // Arrange
        const int nonExistingId = 777;

        // Act
        var result =
                await _projectService.DeleteProjectAsync(
                        nonExistingId);

        // Assert
        Assert.AreEqual($"Проект с ID {nonExistingId} не найден",
                result.Message);
    }

    /// <summary>
    ///     Тест: метод возвращает правильное сообщение BadRequest с названием и
    ///     количеством задач
    /// </summary>
    [TestMethod]
    public async Task
            DeleteProjectAsync_ProjectWithTasks_ReturnsCorrectBadRequestMessageWithNameAndCount()
    {
        // Arrange
        var project = new Project
        {
            Name = "Project With Multiple Tasks",
            Code = "MULTI01", IsActive = true
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var task1 = new WorkTask
        {
            Name = "Task 1", ProjectId = project.Id,
            IsActive = true, Project = null!
        };

        var task2 = new WorkTask
        {
            Name = "Task 2", ProjectId = project.Id,
            IsActive = true, Project = null!
        };

        var task3 = new WorkTask
        {
            Name = "Task 3", ProjectId = project.Id,
            IsActive = false, Project = null!
        };

        _context.WorkTasks.AddRange(task1, task2, task3);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        var result =
                await _projectService.DeleteProjectAsync(
                        project.Id);

        // Assert
        Assert.AreEqual(
                "Нельзя удалить проект 'Project With Multiple Tasks' - у него есть 3 связанных задач",
                result.Message);
    }

    /// <summary>
    ///     Тест: метод логирует начало удаления с ID проекта
    /// </summary>
    [TestMethod]
    public async Task
            DeleteProjectAsync_Invoked_LogsInformationWithProjectId()
    {
        // Arrange
        const int projectId = 123;

        // Act
        await _projectService.DeleteProjectAsync(projectId);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Удаление проекта с ID:") &&
                                o.ToString()!.Contains(
                                        projectId.ToString())),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод логирует успешное удаление с ID проекта
    /// </summary>
    [TestMethod]
    public async Task
            DeleteProjectAsync_SuccessfulDeletion_LogsInformationWithProjectId()
    {
        // Arrange
        var project = new Project
        {
            Name = "Log Project", Code = "LOG01", IsActive = true
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        await _projectService.DeleteProjectAsync(project.Id);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Проект с ID") &&
                                o.ToString()!.Contains(
                                        project.Id.ToString()) &&
                                o.ToString()!.Contains(
                                        "успешно удален")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод логирует предупреждение когда проект не найден
    /// </summary>
    [TestMethod]
    public async Task
            DeleteProjectAsync_NonExistingProject_LogsWarningWithId()
    {
        // Arrange
        const int nonExistingId = 555;

        // Act
        await _projectService.DeleteProjectAsync(nonExistingId);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Warning,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Проект с ID") &&
                                o.ToString()!.Contains(
                                        nonExistingId
                                                .ToString()) &&
                                o.ToString()!.Contains(
                                        "не найден для удаления")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод логирует предупреждение когда есть связанные задачи
    /// </summary>
    [TestMethod]
    public async Task
            DeleteProjectAsync_ProjectWithTasks_LogsWarningWithIdAndTasks()
    {
        // Arrange
        var project = new Project
        {
            Name = "Warning Project", Code = "WARN01",
            IsActive = true
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var task = new WorkTask
        {
            Name = "Warning Task", ProjectId = project.Id,
            IsActive = true, Project = null!
        };

        _context.WorkTasks.Add(task);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        await _projectService.DeleteProjectAsync(project.Id);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Warning,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Нельзя удалить проект с ID") &&
                                o.ToString()!.Contains(
                                        project.Id.ToString()) &&
                                o.ToString()!.Contains(
                                        "есть связанные задачи")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }
}