using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TimeTrackingAPI.Common.DTOs;
using TimeTrackingAPI.Common.Enums;
using TimeTrackingAPI.Common.Models;
using TimeTrackingAPI.Models;
using TimeTrackingAPI.Services.Interfaces;

namespace TimeTrackingAPITests.Controllers.ProjectsController;

/// <summary>
///     Тесты для метода ProjectsController.CreateProject
/// </summary>
[TestClass]
public sealed class ProjectsControllerCreateProjectTests
{
    private TimeTrackingAPI.Controllers.ProjectsController
            _controller = null!;

    private
            ILogger<TimeTrackingAPI.Controllers.ProjectsController> _logger = null!;

    private IProjectService _projectService = null!;

    [TestInitialize]
    public void Setup()
    {
        _projectService = Substitute.For<IProjectService>();
        _logger = Substitute
                .For<ILogger<TimeTrackingAPI.Controllers.ProjectsController>>();

        _controller =
                new TimeTrackingAPI.Controllers.ProjectsController(_projectService,
                                _logger);
    }

    /// <summary>
    ///     Тест: метод создает Project из ProjectCreateDto с правильными свойствами
    /// </summary>
    [TestMethod]
    public async Task
            CreateProject_WithProjectDto_CreatesProjectWithCorrectProperties()
    {
        // Arrange
        var projectDto = new ProjectCreateDto
        {
            Name = "Test Project",
            Code = "TST001",
            IsActive = true
        };

        Project? capturedProject = null;
        var createdProject = new Project
        {
            Id = 1,
            Name = "Test Project",
            Code = "TST001",
            IsActive = true
        };

        _projectService
                .CreateProjectAsync(
                        Arg.Do<Project>(p =>
                                capturedProject = p))
                .Returns(
                        ApiResponse<Project>.Success(
                                createdProject, "Success"));

        // Act
        await _controller.CreateProject(projectDto);

        // Assert
        Assert.IsNotNull(capturedProject);
        Assert.AreEqual("Test Project", capturedProject.Name);
        Assert.AreEqual("TST001", capturedProject.Code);
        Assert.IsTrue(capturedProject.IsActive);
    }

    /// <summary>
    ///     Тест: метод вызывает CreateProjectAsync у сервиса с созданным проектом
    /// </summary>
    [TestMethod]
    public async Task
            CreateProject_WithProjectDto_CallsProjectServiceCreateProjectAsyncWithProject()
    {
        // Arrange
        var projectDto = new ProjectCreateDto
        {
            Name = "Service Project",
            Code = "SRV001",
            IsActive = false
        };

        var createdProject = new Project
        {
            Id = 2,
            Name = "Service Project",
            Code = "SRV001",
            IsActive = false
        };

        _projectService.CreateProjectAsync(Arg.Any<Project>())
                .Returns(
                        ApiResponse<Project>.Success(
                                createdProject, "Success"));

        // Act
        await _controller.CreateProject(projectDto);

        // Assert
        await _projectService.Received(1)
                .CreateProjectAsync(Arg.Any<Project>());
    }

    /// <summary>
    ///     Тест: метод возвращает CreatedAtAction для успешного создания
    /// </summary>
    [TestMethod]
    public async Task
            CreateProject_ServiceReturnsSuccess_ReturnsCreatedAtAction()
    {
        // Arrange
        var projectDto = new ProjectCreateDto
        {
            Name = "Created Project", Code = "CRT001",
            IsActive = true
        };

        var createdProject = new Project
        {
            Id = 123,
            Name = "Created Project",
            Code = "CRT001",
            IsActive = true
        };

        var serviceResponse =
                ApiResponse<Project>.Success(createdProject,
                        "Project created");

        _projectService.CreateProjectAsync(Arg.Any<Project>())
                .Returns(serviceResponse);

        // Act
        var result = await _controller.CreateProject(projectDto);

        // Assert
        Assert.IsInstanceOfType(result.Result,
                typeof(CreatedAtActionResult));
    }

    /// <summary>
    ///     Тест: метод возвращает правильный route для CreatedAtAction
    /// </summary>
    [TestMethod]
    public async Task
            CreateProject_ServiceReturnsSuccess_ReturnsCreatedAtActionWithCorrectRoute()
    {
        // Arrange
        var projectDto = new ProjectCreateDto
        {
            Name = "Route Project", Code = "RTE001",
            IsActive = true
        };

        var createdProject = new Project
        {
            Id = 456,
            Name = "Route Project",
            Code = "RTE001",
            IsActive = true
        };

        var serviceResponse =
                ApiResponse<Project>.Success(createdProject,
                        "Project created");

        _projectService.CreateProjectAsync(Arg.Any<Project>())
                .Returns(serviceResponse);

        // Act
        var result = await _controller.CreateProject(projectDto);

        // Assert
        var createdResult =
                result.Result as CreatedAtActionResult;

        Assert.IsNotNull(createdResult);
        Assert.AreEqual("GetProject", createdResult.ActionName);
        Assert.IsNotNull(createdResult.RouteValues);
        Assert.AreEqual(456, createdResult.RouteValues["id"]);
        Assert.AreSame(serviceResponse, createdResult.Value);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode BadRequest для ошибки валидации
    /// </summary>
    [TestMethod]
    public async Task
            CreateProject_ServiceReturnsBadRequest_ReturnsStatusCode400()
    {
        // Arrange
        var projectDto = new ProjectCreateDto
        {
            Name = "Bad Project", Code = "BAD001",
            IsActive = true
        };

        var serviceResponse = ApiResponse<Project>.Error(
                ApiStatusCode.BadRequest,
                "Project code already exists");

        _projectService.CreateProjectAsync(Arg.Any<Project>())
                .Returns(serviceResponse);

        // Act
        var result = await _controller.CreateProject(projectDto);

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual((int) HttpStatusCode.BadRequest,
                objectResult.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode InternalServerError для ошибки сервера
    /// </summary>
    [TestMethod]
    public async Task
            CreateProject_ServiceReturnsInternalServerError_ReturnsStatusCode500()
    {
        // Arrange
        var projectDto = new ProjectCreateDto
        {
            Name = "Error Project", Code = "ERR001",
            IsActive = true
        };

        var serviceResponse = ApiResponse<Project>.Error(
                ApiStatusCode.InternalServerError,
                "Internal error");

        _projectService.CreateProjectAsync(Arg.Any<Project>())
                .Returns(serviceResponse);

        // Act
        var result = await _controller.CreateProject(projectDto);

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual((int) HttpStatusCode.InternalServerError,
                objectResult.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает ApiResponse в качестве значения для всех случаев
    /// </summary>
    [TestMethod]
    public async Task
            CreateProject_ServiceResponse_ReturnsActionResultWithApiResponse()
    {
        // Arrange
        var projectDto = new ProjectCreateDto
        {
            Name = "Response Project", Code = "RSP001",
            IsActive = true
        };

        var createdProject = new Project
        {
            Id = 3,
            Name = "Response Project",
            Code = "RSP001",
            IsActive = true
        };

        var serviceResponse =
                ApiResponse<Project>.Success(createdProject,
                        "Success");

        _projectService.CreateProjectAsync(Arg.Any<Project>())
                .Returns(serviceResponse);

        // Act
        var result = await _controller.CreateProject(projectDto);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);

        var createdResult =
                result.Result as CreatedAtActionResult;

        Assert.IsNotNull(createdResult);
        Assert.IsInstanceOfType(createdResult.Value,
                typeof(ApiResponse<Project>));
    }

    /// <summary>
    ///     Тест: метод логирует создание проекта с названием
    /// </summary>
    [TestMethod]
    public async Task
            CreateProject_WithProjectDto_LogsInformationWithName()
    {
        // Arrange
        var projectDto = new ProjectCreateDto
        {
            Name = "Log Test Project",
            Code = "LOG001",
            IsActive = true
        };

        var createdProject = new Project
        {
            Id = 4,
            Name = "Log Test Project",
            Code = "LOG001",
            IsActive = true
        };

        _projectService.CreateProjectAsync(Arg.Any<Project>())
                .Returns(
                        ApiResponse<Project>.Success(
                                createdProject, "Success"));

        // Act
        await _controller.CreateProject(projectDto);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Получен запрос на создание проекта:") &&
                                o.ToString()!.Contains(
                                        "Log Test Project")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }
}