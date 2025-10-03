using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TimeTrackingAPI.Common.Enums;
using TimeTrackingAPI.Data;
using TimeTrackingAPI.Models;

namespace TimeTrackingAPITests.Services.ProjectService;

/// <summary>
///     Тесты для метода ProjectService.GetAllProjectsAsync
/// </summary>
[TestClass]
public sealed class ProjectServiceGetAllProjectsAsyncTests
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
    ///     Тест: метод возвращает все проекты
    /// </summary>
    [TestMethod]
    public async Task
            GetAllProjectsAsync_MultipleProjects_ReturnsAllProjects()
    {
        // Arrange
        var project1 = new Project
        {
            Name = "Project Alpha", Code = "ALPHA",
            IsActive = true
        };

        var project2 = new Project
        {
            Name = "Project Beta", Code = "BETA",
            IsActive = false
        };

        var project3 = new Project
        {
            Name = "Project Gamma", Code = "GAMMA",
            IsActive = true
        };

        _context.Projects.AddRange(project1, project2, project3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _projectService.GetAllProjectsAsync();

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(3,
                result.Data
                        .Count); // Все проекты, включая неактивные
    }

    /// <summary>
    ///     Тест: метод возвращает проекты отсортированные по названию по возрастанию
    /// </summary>
    [TestMethod]
    public async Task
            GetAllProjectsAsync_UnsortedProjects_ReturnsSortedByNameAscending()
    {
        // Arrange
        var projectZ = new Project
        {
            Name = "Z Project", Code = "ZPROJ", IsActive = true
        };

        var projectA = new Project
        {
            Name = "A Project", Code = "APROJ", IsActive = true
        };

        var projectM = new Project
        {
            Name = "M Project", Code = "MPROJ", IsActive = true
        };

        _context.Projects.AddRange(projectZ, projectA, projectM);
        await _context.SaveChangesAsync();

        // Act
        var result = await _projectService.GetAllProjectsAsync();

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual("A Project", result.Data[0].Name);
        Assert.AreEqual("M Project", result.Data[1].Name);
        Assert.AreEqual("Z Project", result.Data[2].Name);
    }

    /// <summary>
    ///     Тест: метод возвращает IsSuccess true при успешном выполнении
    /// </summary>
    [TestMethod]
    public async Task
            GetAllProjectsAsync_SuccessfulExecution_ReturnsIsSuccessTrue()
    {
        // Arrange
        // База данных может быть пустой

        // Act
        var result = await _projectService.GetAllProjectsAsync();

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode Success при успешном выполнении
    /// </summary>
    [TestMethod]
    public async Task
            GetAllProjectsAsync_SuccessfulExecution_ReturnsStatusCodeSuccess()
    {
        // Arrange
        // База данных может быть пустой

        // Act
        var result = await _projectService.GetAllProjectsAsync();

        // Assert
        Assert.AreEqual(ApiStatusCode.Success,
                result.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает правильное сообщение с количеством проектов
    /// </summary>
    [TestMethod]
    public async Task
            GetAllProjectsAsync_VariousProjectCounts_ReturnsMessageWithCorrectCount()
    {
        // Arrange
        var project1 = new Project
        {
            Name = "Project One", Code = "PROJ1", IsActive = true
        };

        var project2 = new Project
        {
            Name = "Project Two", Code = "PROJ2",
            IsActive = false
        };

        _context.Projects.AddRange(project1, project2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _projectService.GetAllProjectsAsync();

        // Assert
        Assert.AreEqual("Получено 2 проектов", result.Message);
    }

    /// <summary>
    ///     Тест: метод логирует начало операции
    /// </summary>
    [TestMethod]
    public async Task
            GetAllProjectsAsync_Invoked_LogsInformationAboutStart()
    {
        // Arrange
        // База данных может быть пустой

        // Act
        await _projectService.GetAllProjectsAsync();

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Запрос на получение всех проектов")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод логирует успешное завершение с количеством проектов
    /// </summary>
    [TestMethod]
    public async Task
            GetAllProjectsAsync_SuccessfulExecution_LogsInformationWithCount()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project", Code = "TEST", IsActive = true
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        await _projectService.GetAllProjectsAsync();

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
                                        "проектов")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Параметризованный тест: метод возвращает корректное количество проектов
    /// </summary>
    [DataTestMethod]
    [DataRow(0, DisplayName = "Нет проектов")]
    [DataRow(1, DisplayName = "Один проект")]
    [DataRow(3, DisplayName = "Три проекта")]
    [DataRow(5, DisplayName = "Пять проектов")]
    public async Task
            GetAllProjectsAsync_VariousProjectCounts_ReturnsCorrectCount(
                    int projectCount)
    {
        // Arrange
        for (var i = 0; i < projectCount; i++)
        {
            var project = new Project
            {
                Name = $"Project {i + 1}",
                Code = $"PROJ{i + 1:00}",
                IsActive =
                        i % 2 ==
                        0 // Чередуем активные и неактивные
            };

            _context.Projects.Add(project);
        }

        if (projectCount > 0)
            await _context.SaveChangesAsync();

        // Act
        var result = await _projectService.GetAllProjectsAsync();

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(projectCount,
                result.Data
                        .Count); // Все проекты, включая неактивные

        Assert.IsTrue(result.IsSuccess);

        if (projectCount > 0)
                // Проверяем что проекты отсортированы по названию
            for (var i = 0; i < result.Data.Count - 1; i++)
                Assert.IsTrue(string.Compare(result.Data[i].Name,
                                      result.Data[i + 1].Name,
                                      StringComparison
                                              .Ordinal) <=
                              0);
    }
}