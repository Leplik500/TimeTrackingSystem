using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TimeTrackingAPI.Common.Enums;
using TimeTrackingAPI.Data;
using TimeTrackingAPI.Models;

namespace TimeTrackingAPITests.Services.ProjectService;

/// <summary>
///     Тесты для метода ProjectService.UpdateProjectAsync
/// </summary>
[TestClass]
public sealed class ProjectServiceUpdateProjectAsyncTests
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
    ///     Тест: метод обновляет существующий проект с уникальным кодом
    /// </summary>
    [TestMethod]
    public async Task
            UpdateProjectAsync_ExistingProjectUniqueCode_UpdatesProject()
    {
        // Arrange
        var existingProject = new Project
        {
            Name = "Old Name", Code = "OLD01", IsActive = true
        };

        _context.Projects.Add(existingProject);
        await _context.SaveChangesAsync();

        var updateData = new Project
        {
            Name = "New Name", Code = "NEW01", IsActive = false
        };

        // Act
        var result =
                await _projectService.UpdateProjectAsync(
                        existingProject.Id, updateData);

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual("New Name", result.Data.Name);
        Assert.AreEqual("NEW01", result.Data.Code);
        Assert.IsFalse(result.Data.IsActive);
    }

    /// <summary>
    ///     Тест: метод возвращает NotFound для несуществующего проекта
    /// </summary>
    [TestMethod]
    public async Task
            UpdateProjectAsync_NonExistingProject_ReturnsNotFound()
    {
        // Arrange
        const int nonExistingId = 999;
        var updateData = new Project
        {
            Name = "New Name", Code = "NEW01", IsActive = true
        };

        // Act
        var result =
                await _projectService.UpdateProjectAsync(
                        nonExistingId, updateData);

        // Assert
        Assert.IsNull(result.Data);
    }

    /// <summary>
    ///     Тест: метод возвращает BadRequest при дублировании кода с другим проектом
    /// </summary>
    [TestMethod]
    public async Task
            UpdateProjectAsync_DuplicateCodeWithOtherProject_ReturnsBadRequest()
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

        _context.Projects.AddRange(project1, project2);
        await _context.SaveChangesAsync();

        // Пытаемся обновить project2 с кодом project1
        var updateData = new Project
        {
            Name = "Updated Name", Code = "PROJ01",
            IsActive = true
        };

        // Act
        var result =
                await _projectService.UpdateProjectAsync(
                        project2.Id, updateData);

        // Assert
        Assert.IsNull(result.Data);
    }

    /// <summary>
    ///     Тест: метод позволяет сохранить тот же код для того же проекта
    /// </summary>
    [TestMethod]
    public async Task
            UpdateProjectAsync_SameCodeForSameProject_AllowsUpdate()
    {
        // Arrange
        var existingProject = new Project
        {
            Name = "Original Name", Code = "SAME01",
            IsActive = true
        };

        _context.Projects.Add(existingProject);
        await _context.SaveChangesAsync();

        // Обновляем с тем же кодом, но другим названием
        var updateData = new Project
        {
            Name = "Updated Name", Code = "SAME01",
            IsActive = false
        };

        // Act
        var result =
                await _projectService.UpdateProjectAsync(
                        existingProject.Id, updateData);

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual("Updated Name", result.Data.Name);
        Assert.AreEqual("SAME01", result.Data.Code);
        Assert.IsFalse(result.Data.IsActive);
    }

    /// <summary>
    ///     Тест: метод обновляет все поля проекта
    /// </summary>
    [TestMethod]
    public async Task
            UpdateProjectAsync_ValidUpdate_UpdatesAllFields()
    {
        // Arrange
        var existingProject = new Project
        {
            Name = "Old Name", Code = "OLD01", IsActive = true
        };

        _context.Projects.Add(existingProject);
        await _context.SaveChangesAsync();
        var projectId = existingProject.Id;

        var updateData = new Project
        {
            Name = "New Name", Code = "NEW01", IsActive = false
        };

        // Act
        await _projectService.UpdateProjectAsync(projectId,
                updateData);

        // Assert - проверяем что все поля обновлены в БД
        _context.ChangeTracker.Clear();
        var updatedProject =
                await _context.Projects.FindAsync(projectId);

        Assert.IsNotNull(updatedProject);
        Assert.AreEqual("New Name", updatedProject.Name);
        Assert.AreEqual("NEW01", updatedProject.Code);
        Assert.IsFalse(updatedProject.IsActive);
    }

    /// <summary>
    ///     Тест: метод возвращает IsSuccess true при успешном обновлении
    /// </summary>
    [TestMethod]
    public async Task
            UpdateProjectAsync_SuccessfulUpdate_ReturnsIsSuccessTrue()
    {
        // Arrange
        var existingProject = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        _context.Projects.Add(existingProject);
        await _context.SaveChangesAsync();

        var updateData = new Project
        {
            Name = "Updated Project", Code = "UPDATED01",
            IsActive = false
        };

        // Act
        var result =
                await _projectService.UpdateProjectAsync(
                        existingProject.Id, updateData);

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    /// <summary>
    ///     Тест: метод возвращает IsSuccess false для несуществующего проекта
    /// </summary>
    [TestMethod]
    public async Task
            UpdateProjectAsync_NonExistingProject_ReturnsIsSuccessFalse()
    {
        // Arrange
        const int nonExistingId = 999;
        var updateData = new Project
        {
            Name = "New Name", Code = "NEW01", IsActive = true
        };

        // Act
        var result =
                await _projectService.UpdateProjectAsync(
                        nonExistingId, updateData);

        // Assert
        Assert.IsFalse(result.IsSuccess);
    }

    /// <summary>
    ///     Тест: метод возвращает IsSuccess false при дублировании кода
    /// </summary>
    [TestMethod]
    public async Task
            UpdateProjectAsync_DuplicateCode_ReturnsIsSuccessFalse()
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

        _context.Projects.AddRange(project1, project2);
        await _context.SaveChangesAsync();

        var updateData = new Project
        {
            Name = "Updated Name", Code = "PROJ01",
            IsActive = true
        };

        // Act
        var result =
                await _projectService.UpdateProjectAsync(
                        project2.Id, updateData);

        // Assert
        Assert.IsFalse(result.IsSuccess);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode Success при успешном обновлении
    /// </summary>
    [TestMethod]
    public async Task
            UpdateProjectAsync_SuccessfulUpdate_ReturnsStatusCodeSuccess()
    {
        // Arrange
        var existingProject = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        _context.Projects.Add(existingProject);
        await _context.SaveChangesAsync();

        var updateData = new Project
        {
            Name = "Updated Project", Code = "UPDATED01",
            IsActive = false
        };

        // Act
        var result =
                await _projectService.UpdateProjectAsync(
                        existingProject.Id, updateData);

        // Assert
        Assert.AreEqual(ApiStatusCode.Success,
                result.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode NotFound для несуществующего проекта
    /// </summary>
    [TestMethod]
    public async Task
            UpdateProjectAsync_NonExistingProject_ReturnsStatusCodeNotFound()
    {
        // Arrange
        const int nonExistingId = 999;
        var updateData = new Project
        {
            Name = "New Name", Code = "NEW01", IsActive = true
        };

        // Act
        var result =
                await _projectService.UpdateProjectAsync(
                        nonExistingId, updateData);

        // Assert
        Assert.AreEqual(ApiStatusCode.NotFound,
                result.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode BadRequest при дублировании кода
    /// </summary>
    [TestMethod]
    public async Task
            UpdateProjectAsync_DuplicateCode_ReturnsStatusCodeBadRequest()
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

        _context.Projects.AddRange(project1, project2);
        await _context.SaveChangesAsync();

        var updateData = new Project
        {
            Name = "Updated Name", Code = "PROJ01",
            IsActive = true
        };

        // Act
        var result =
                await _projectService.UpdateProjectAsync(
                        project2.Id, updateData);

        // Assert
        Assert.AreEqual(ApiStatusCode.BadRequest,
                result.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает правильное сообщение об успехе
    /// </summary>
    [TestMethod]
    public async Task
            UpdateProjectAsync_SuccessfulUpdate_ReturnsCorrectSuccessMessage()
    {
        // Arrange
        var existingProject = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        _context.Projects.Add(existingProject);
        await _context.SaveChangesAsync();

        var updateData = new Project
        {
            Name = "Updated Project", Code = "UPDATED01",
            IsActive = false
        };

        // Act
        var result =
                await _projectService.UpdateProjectAsync(
                        existingProject.Id, updateData);

        // Assert
        Assert.AreEqual("Проект успешно обновлен",
                result.Message);
    }

    /// <summary>
    ///     Тест: метод возвращает правильное сообщение NotFound с ID
    /// </summary>
    [TestMethod]
    public async Task
            UpdateProjectAsync_NonExistingProject_ReturnsCorrectNotFoundMessageWithId()
    {
        // Arrange
        const int nonExistingId = 777;
        var updateData = new Project
        {
            Name = "New Name", Code = "NEW01", IsActive = true
        };

        // Act
        var result =
                await _projectService.UpdateProjectAsync(
                        nonExistingId, updateData);

        // Assert
        Assert.AreEqual($"Проект с ID {nonExistingId} не найден",
                result.Message);
    }

    /// <summary>
    ///     Тест: метод возвращает правильное сообщение BadRequest с кодом
    /// </summary>
    [TestMethod]
    public async Task
            UpdateProjectAsync_DuplicateCode_ReturnsCorrectBadRequestMessageWithCode()
    {
        // Arrange
        var project1 = new Project
        {
            Name = "Project 1", Code = "BADREQ01",
            IsActive = true
        };

        var project2 = new Project
        {
            Name = "Project 2", Code = "PROJ02", IsActive = true
        };

        _context.Projects.AddRange(project1, project2);
        await _context.SaveChangesAsync();

        var updateData = new Project
        {
            Name = "Updated Name", Code = "BADREQ01",
            IsActive = true
        };

        // Act
        var result =
                await _projectService.UpdateProjectAsync(
                        project2.Id, updateData);

        // Assert
        Assert.AreEqual(
                "Проект с кодом 'BADREQ01' уже существует",
                result.Message);
    }

    /// <summary>
    ///     Тест: метод логирует начало обновления с ID проекта
    /// </summary>
    [TestMethod]
    public async Task
            UpdateProjectAsync_Invoked_LogsInformationWithProjectId()
    {
        // Arrange
        const int projectId = 123;
        var updateData = new Project
        {
            Name = "New Name", Code = "NEW01", IsActive = true
        };

        // Act
        await _projectService.UpdateProjectAsync(projectId,
                updateData);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Обновление проекта с ID:") &&
                                o.ToString()!.Contains(
                                        projectId.ToString())),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод логирует успешное обновление с ID проекта
    /// </summary>
    [TestMethod]
    public async Task
            UpdateProjectAsync_SuccessfulUpdate_LogsInformationWithProjectId()
    {
        // Arrange
        var existingProject = new Project
        {
            Name = "Test Project", Code = "TEST01",
            IsActive = true
        };

        _context.Projects.Add(existingProject);
        await _context.SaveChangesAsync();

        var updateData = new Project
        {
            Name = "Updated Project", Code = "UPDATED01",
            IsActive = false
        };

        // Act
        await _projectService.UpdateProjectAsync(
                existingProject.Id, updateData);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Проект с ID") &&
                                o.ToString()!.Contains(
                                        existingProject.Id
                                                .ToString()) &&
                                o.ToString()!.Contains(
                                        "успешно обновлен")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод логирует предупреждение когда проект не найден
    /// </summary>
    [TestMethod]
    public async Task
            UpdateProjectAsync_NonExistingProject_LogsWarningWithId()
    {
        // Arrange
        const int nonExistingId = 555;
        var updateData = new Project
        {
            Name = "New Name", Code = "NEW01", IsActive = true
        };

        // Act
        await _projectService.UpdateProjectAsync(nonExistingId,
                updateData);

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
                                        "не найден для обновления")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод логирует предупреждение при дублировании кода
    /// </summary>
    [TestMethod]
    public async Task
            UpdateProjectAsync_DuplicateCode_LogsWarningWithCode()
    {
        // Arrange
        var project1 = new Project
        {
            Name = "Project 1", Code = "LOGWARN01",
            IsActive = true
        };

        var project2 = new Project
        {
            Name = "Project 2", Code = "PROJ02", IsActive = true
        };

        _context.Projects.AddRange(project1, project2);
        await _context.SaveChangesAsync();

        var updateData = new Project
        {
            Name = "Updated Name", Code = "LOGWARN01",
            IsActive = true
        };

        // Act
        await _projectService.UpdateProjectAsync(project2.Id,
                updateData);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Warning,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Проект с кодом") &&
                                o.ToString()!.Contains(
                                        "LOGWARN01") &&
                                o.ToString()!.Contains(
                                        "уже существует")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }
}