using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TimeTrackingAPI.Common.Enums;
using TimeTrackingAPI.Data;
using TimeTrackingAPI.Models;

namespace TimeTrackingAPITests.Services.ProjectService;

/// <summary>
///     Тесты для метода ProjectService.CreateProjectAsync
/// </summary>
[TestClass]
public sealed class ProjectServiceCreateProjectAsyncTests
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
    ///     Тест: метод создает новый проект с уникальным кодом и сохраняет в БД
    /// </summary>
    [TestMethod]
    public async Task
            CreateProjectAsync_UniqueCode_CreatesAndSavesProject()
    {
        // Arrange
        var project = new Project
        {
            Name = "New Project",
            Code = "UNIQUE01",
            IsActive = true
        };

        // Act
        var result =
                await _projectService
                        .CreateProjectAsync(project);

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual("New Project", result.Data.Name);
        Assert.AreEqual("UNIQUE01", result.Data.Code);
        Assert.IsTrue(result.Data.Id > 0); // ID сгенерирован

        // Проверяем что проект сохранен в БД
        var savedProject =
                await _context.Projects.FirstOrDefaultAsync(p =>
                        p.Code == "UNIQUE01");

        Assert.IsNotNull(savedProject);
        Assert.AreEqual("New Project", savedProject.Name);
    }

    /// <summary>
    ///     Тест: метод возвращает BadRequest для дублирующегося кода проекта
    /// </summary>
    [TestMethod]
    public async Task
            CreateProjectAsync_DuplicateCode_ReturnsBadRequest()
    {
        // Arrange
        var existingProject = new Project
        {
            Name = "Existing Project", Code = "DUPLICATE",
            IsActive = true
        };

        _context.Projects.Add(existingProject);
        await _context.SaveChangesAsync();

        var newProject = new Project
        {
            Name = "New Project", Code = "DUPLICATE",
            IsActive = true
        };

        // Act
        var result =
                await _projectService.CreateProjectAsync(
                        newProject);

        // Assert
        Assert.IsNull(result.Data);
    }

    /// <summary>
    ///     Тест: метод возвращает IsSuccess true при успешном создании
    /// </summary>
    [TestMethod]
    public async Task
            CreateProjectAsync_SuccessfulCreation_ReturnsIsSuccessTrue()
    {
        // Arrange
        var project = new Project
        {
            Name = "Success Project", Code = "SUCCESS01",
            IsActive = true
        };

        // Act
        var result =
                await _projectService
                        .CreateProjectAsync(project);

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    /// <summary>
    ///     Тест: метод возвращает IsSuccess false при дублировании кода
    /// </summary>
    [TestMethod]
    public async Task
            CreateProjectAsync_DuplicateCode_ReturnsIsSuccessFalse()
    {
        // Arrange
        var existingProject = new Project
        {
            Name = "Existing Project", Code = "DUPLICATE",
            IsActive = true
        };

        _context.Projects.Add(existingProject);
        await _context.SaveChangesAsync();

        var newProject = new Project
        {
            Name = "New Project", Code = "DUPLICATE",
            IsActive = true
        };

        // Act
        var result =
                await _projectService.CreateProjectAsync(
                        newProject);

        // Assert
        Assert.IsFalse(result.IsSuccess);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode Success при успешном создании
    /// </summary>
    [TestMethod]
    public async Task
            CreateProjectAsync_SuccessfulCreation_ReturnsStatusCodeSuccess()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        // Act
        var result =
                await _projectService
                        .CreateProjectAsync(project);

        // Assert
        Assert.AreEqual(ApiStatusCode.Success,
                result.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode BadRequest при дублировании кода
    /// </summary>
    [TestMethod]
    public async Task
            CreateProjectAsync_DuplicateCode_ReturnsStatusCodeBadRequest()
    {
        // Arrange
        var existingProject = new Project
        {
            Name = "Existing Project", Code = "DUPLICATE",
            IsActive = true
        };

        _context.Projects.Add(existingProject);
        await _context.SaveChangesAsync();

        var newProject = new Project
        {
            Name = "New Project", Code = "DUPLICATE",
            IsActive = true
        };

        // Act
        var result =
                await _projectService.CreateProjectAsync(
                        newProject);

        // Assert
        Assert.AreEqual(ApiStatusCode.BadRequest,
                result.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает правильное сообщение об успехе
    /// </summary>
    [TestMethod]
    public async Task
            CreateProjectAsync_SuccessfulCreation_ReturnsCorrectSuccessMessage()
    {
        // Arrange
        var project = new Project
        {
            Name = "Message Project", Code = "MESSAGE01",
            IsActive = true
        };

        // Act
        var result =
                await _projectService
                        .CreateProjectAsync(project);

        // Assert
        Assert.AreEqual("Проект успешно создан", result.Message);
    }

    /// <summary>
    ///     Тест: метод возвращает правильное сообщение об ошибке с кодом
    /// </summary>
    [TestMethod]
    public async Task
            CreateProjectAsync_DuplicateCode_ReturnsCorrectErrorMessageWithCode()
    {
        // Arrange
        var existingProject = new Project
        {
            Name = "Existing Project", Code = "ERROR01",
            IsActive = true
        };

        _context.Projects.Add(existingProject);
        await _context.SaveChangesAsync();

        var newProject = new Project
        {
            Name = "New Project", Code = "ERROR01",
            IsActive = true
        };

        // Act
        var result =
                await _projectService.CreateProjectAsync(
                        newProject);

        // Assert
        Assert.AreEqual(
                "Проект с кодом 'ERROR01' уже существует",
                result.Message);
    }

    /// <summary>
    ///     Тест: метод логирует начало создания с названием проекта
    /// </summary>
    [TestMethod]
    public async Task
            CreateProjectAsync_Invoked_LogsInformationWithProjectName()
    {
        // Arrange
        var project = new Project
        {
            Name = "Log Test Project", Code = "LOG01",
            IsActive = true
        };

        // Act
        await _projectService.CreateProjectAsync(project);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Создание нового проекта:") &&
                                o.ToString()!.Contains(
                                        "Log Test Project")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод логирует успешное создание с сгенерированным ID
    /// </summary>
    [TestMethod]
    public async Task
            CreateProjectAsync_SuccessfulCreation_LogsInformationWithGeneratedId()
    {
        // Arrange
        var project = new Project
        {
            Name = "ID Log Project", Code = "IDLOG01",
            IsActive = true
        };

        // Act
        var result =
                await _projectService
                        .CreateProjectAsync(project);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Проект успешно создан с ID:") &&
                                o.ToString()!.Contains(
                                        result.Data!.Id
                                                .ToString())),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод логирует предупреждение при дублировании кода
    /// </summary>
    [TestMethod]
    public async Task
            CreateProjectAsync_DuplicateCode_LogsWarningWithCode()
    {
        // Arrange
        var existingProject = new Project
        {
            Name = "Existing Project", Code = "WARNING01",
            IsActive = true
        };

        _context.Projects.Add(existingProject);
        await _context.SaveChangesAsync();

        var newProject = new Project
        {
            Name = "New Project", Code = "WARNING01",
            IsActive = true
        };

        // Act
        await _projectService.CreateProjectAsync(newProject);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Warning,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Проект с кодом") &&
                                o.ToString()!.Contains(
                                        "WARNING01") &&
                                o.ToString()!.Contains(
                                        "уже существует")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }
}