using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TimeTrackingAPI.Common.Enums;
using TimeTrackingAPI.Data;
using TimeTrackingAPI.Models;

namespace TimeTrackingAPITests.Services.ProjectService;

/// <summary>
///     Тесты для метода ProjectService.GetProjectByIdAsync
/// </summary>
[TestClass]
public sealed class ProjectServiceGetProjectByIdAsyncTests
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
    ///     Тест: метод возвращает проект когда ID существует
    /// </summary>
    [TestMethod]
    public async Task
            GetProjectByIdAsync_ExistingId_ReturnsProject()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        var projectId = project.Id;

        // Act
        var result =
                await _projectService.GetProjectByIdAsync(
                        projectId);

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(projectId, result.Data.Id);
        Assert.AreEqual("Test Project", result.Data.Name);
    }

    /// <summary>
    ///     Тест: метод возвращает NotFound когда ID не существует
    /// </summary>
    [TestMethod]
    public async Task
            GetProjectByIdAsync_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        const int nonExistingId = 999;

        // Act
        var result =
                await _projectService.GetProjectByIdAsync(
                        nonExistingId);

        // Assert
        Assert.IsNull(result.Data);
    }

    /// <summary>
    ///     Тест: метод возвращает IsSuccess true для существующего проекта
    /// </summary>
    [TestMethod]
    public async Task
            GetProjectByIdAsync_ExistingProject_ReturnsIsSuccessTrue()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _projectService.GetProjectByIdAsync(
                        project.Id);

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    /// <summary>
    ///     Тест: метод возвращает IsSuccess false для несуществующего проекта
    /// </summary>
    [TestMethod]
    public async Task
            GetProjectByIdAsync_NonExistingProject_ReturnsIsSuccessFalse()
    {
        // Arrange
        const int nonExistingId = 999;

        // Act
        var result =
                await _projectService.GetProjectByIdAsync(
                        nonExistingId);

        // Assert
        Assert.IsFalse(result.IsSuccess);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode Success для существующего проекта
    /// </summary>
    [TestMethod]
    public async Task
            GetProjectByIdAsync_ExistingProject_ReturnsStatusCodeSuccess()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _projectService.GetProjectByIdAsync(
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
            GetProjectByIdAsync_NonExistingProject_ReturnsStatusCodeNotFound()
    {
        // Arrange
        const int nonExistingId = 999;

        // Act
        var result =
                await _projectService.GetProjectByIdAsync(
                        nonExistingId);

        // Assert
        Assert.AreEqual(ApiStatusCode.NotFound,
                result.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает правильное сообщение об успехе
    /// </summary>
    [TestMethod]
    public async Task
            GetProjectByIdAsync_ExistingProject_ReturnsCorrectSuccessMessage()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        var result =
                await _projectService.GetProjectByIdAsync(
                        project.Id);

        // Assert
        Assert.AreEqual("Проект успешно получен",
                result.Message);
    }

    /// <summary>
    ///     Тест: метод возвращает правильное сообщение об ошибке с ID
    /// </summary>
    [TestMethod]
    public async Task
            GetProjectByIdAsync_NonExistingProject_ReturnsCorrectErrorMessageWithId()
    {
        // Arrange
        const int nonExistingId = 999;

        // Act
        var result =
                await _projectService.GetProjectByIdAsync(
                        nonExistingId);

        // Assert
        Assert.AreEqual($"Проект с ID {nonExistingId} не найден",
                result.Message);
    }

    /// <summary>
    ///     Тест: метод логирует запрос с переданным ID
    /// </summary>
    [TestMethod]
    public async Task
            GetProjectByIdAsync_Invoked_LogsInformationWithRequestedId()
    {
        // Arrange
        const int requestedId = 123;

        // Act
        await _projectService.GetProjectByIdAsync(requestedId);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Запрос на получение проекта с ID:") &&
                                o.ToString()!.Contains(
                                        requestedId.ToString())),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод логирует успешное нахождение с ID и названием проекта
    /// </summary>
    [TestMethod]
    public async Task
            GetProjectByIdAsync_ExistingProject_LogsInformationWithIdAndName()
    {
        // Arrange
        var project = new Project
        {
            Name = "Success Project", Code = "SUCCESS",
            IsActive = true
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        await _projectService.GetProjectByIdAsync(project.Id);

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
                                o.ToString()!
                                        .Contains("найден:") &&
                                o.ToString()!.Contains(
                                        "Success Project")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод логирует предупреждение когда проект не найден
    /// </summary>
    [TestMethod]
    public async Task
            GetProjectByIdAsync_NonExistingProject_LogsWarningWithId()
    {
        // Arrange
        const int nonExistingId = 777;

        // Act
        await _projectService.GetProjectByIdAsync(nonExistingId);

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
                                        "не найден")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод возвращает null в Data для несуществующего проекта
    /// </summary>
    [TestMethod]
    public async Task
            GetProjectByIdAsync_NonExistingProject_ReturnsNullData()
    {
        // Arrange
        const int nonExistingId = 555;

        // Act
        var result =
                await _projectService.GetProjectByIdAsync(
                        nonExistingId);

        // Assert
        Assert.IsNull(result.Data);
    }
}