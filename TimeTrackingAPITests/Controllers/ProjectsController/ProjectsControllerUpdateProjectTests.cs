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
///     Тесты для метода ProjectsController.UpdateProject
/// </summary>
[TestClass]
public sealed class ProjectsControllerUpdateProjectTests
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
    ///     Тест: метод создает Project из параметров с правильными свойствами
    /// </summary>
    [TestMethod]
    public async Task
            UpdateProject_WithIdAndDto_CreatesProjectWithCorrectProperties()
    {
        // Arrange
        const int projectId = 42;
        var projectDto = new ProjectUpdateDto
        {
            Name = "Updated Project",
            Code = "UPD001",
            IsActive = false
        };

        Project? capturedProject = null;
        _projectService.UpdateProjectAsync(projectId,
                        Arg.Do<Project>(p =>
                                capturedProject = p))
                .Returns(
                        ApiResponse<Project>.Success(null!,
                                "Success"));

        // Act
        await _controller.UpdateProject(projectId, projectDto);

        // Assert
        Assert.IsNotNull(capturedProject);
        Assert.AreEqual(42,
                capturedProject.Id); // ID из route параметра

        Assert.AreEqual("Updated Project", capturedProject.Name);
        Assert.AreEqual("UPD001", capturedProject.Code);
        Assert.IsFalse(capturedProject.IsActive);
    }

    /// <summary>
    ///     Тест: метод вызывает UpdateProjectAsync у сервиса с ID и созданным проектом
    /// </summary>
    [TestMethod]
    public async Task
            UpdateProject_WithIdAndDto_CallsProjectServiceUpdateProjectAsyncWithIdAndProject()
    {
        // Arrange
        const int projectId = 123;
        var projectDto = new ProjectUpdateDto
        {
            Name = "Service Project",
            Code = "SRV001",
            IsActive = true
        };

        _projectService
                .UpdateProjectAsync(projectId,
                        Arg.Any<Project>())
                .Returns(
                        ApiResponse<Project>.Success(null!,
                                "Success"));

        // Act
        await _controller.UpdateProject(projectId, projectDto);

        // Assert
        await _projectService.Received(1)
                .UpdateProjectAsync(projectId,
                        Arg.Any<Project>());
    }

    /// <summary>
    ///     Тест: метод возвращает результат сервиса без изменений
    /// </summary>
    [TestMethod]
    public async Task
            UpdateProject_ServiceReturnsData_ReturnsServiceResponseUnchanged()
    {
        // Arrange
        const int projectId = 99;
        var projectDto = new ProjectUpdateDto
        {
            Name = "Test", Code = "TST001", IsActive = true
        };

        var serviceResponse =
                ApiResponse<Project>.Success(null!,
                        "Project updated");

        _projectService
                .UpdateProjectAsync(projectId,
                        Arg.Any<Project>())
                .Returns(serviceResponse);

        // Act
        var result =
                await _controller.UpdateProject(projectId,
                        projectDto);

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        var apiResponse =
                objectResult.Value as ApiResponse<Project>;

        Assert.IsNotNull(apiResponse);

        Assert.AreSame(serviceResponse, apiResponse);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode Success для успешного обновления
    /// </summary>
    [TestMethod]
    public async Task
            UpdateProject_ServiceReturnsSuccess_ReturnsStatusCode200()
    {
        // Arrange
        const int projectId = 50;
        var projectDto = new ProjectUpdateDto
        {
            Name = "Success Project", Code = "SUC001",
            IsActive = true
        };

        var serviceResponse =
                ApiResponse<Project>.Success(null!,
                        "Success message");

        _projectService
                .UpdateProjectAsync(projectId,
                        Arg.Any<Project>())
                .Returns(serviceResponse);

        // Act
        var result =
                await _controller.UpdateProject(projectId,
                        projectDto);

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual((int) HttpStatusCode.OK,
                objectResult.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode NotFound для не найденного проекта
    /// </summary>
    [TestMethod]
    public async Task
            UpdateProject_ServiceReturnsNotFound_ReturnsStatusCode404()
    {
        // Arrange
        const int projectId = 999;
        var projectDto = new ProjectUpdateDto
        {
            Name = "Not Found Project", Code = "NF001",
            IsActive = true
        };

        var serviceResponse = ApiResponse<Project>.Error(
                ApiStatusCode.NotFound,
                "Project not found");

        _projectService
                .UpdateProjectAsync(projectId,
                        Arg.Any<Project>())
                .Returns(serviceResponse);

        // Act
        var result =
                await _controller.UpdateProject(projectId,
                        projectDto);

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual((int) HttpStatusCode.NotFound,
                objectResult.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode BadRequest для ошибки валидации
    /// </summary>
    [TestMethod]
    public async Task
            UpdateProject_ServiceReturnsBadRequest_ReturnsStatusCode400()
    {
        // Arrange
        const int projectId = 75;
        var projectDto = new ProjectUpdateDto
        {
            Name = "Bad Project", Code = "BAD001",
            IsActive = true
        };

        var serviceResponse = ApiResponse<Project>.Error(
                ApiStatusCode.BadRequest,
                "Project code already exists");

        _projectService
                .UpdateProjectAsync(projectId,
                        Arg.Any<Project>())
                .Returns(serviceResponse);

        // Act
        var result =
                await _controller.UpdateProject(projectId,
                        projectDto);

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
            UpdateProject_ServiceReturnsInternalServerError_ReturnsStatusCode500()
    {
        // Arrange
        const int projectId = 200;
        var projectDto = new ProjectUpdateDto
        {
            Name = "Error Project", Code = "ERR001",
            IsActive = false
        };

        var serviceResponse = ApiResponse<Project>.Error(
                ApiStatusCode.InternalServerError,
                "Internal error");

        _projectService
                .UpdateProjectAsync(projectId,
                        Arg.Any<Project>())
                .Returns(serviceResponse);

        // Act
        var result =
                await _controller.UpdateProject(projectId,
                        projectDto);

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual((int) HttpStatusCode.InternalServerError,
                objectResult.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает ActionResult с ApiResponse в качестве значения
    /// </summary>
    [TestMethod]
    public async Task
            UpdateProject_ServiceResponse_ReturnsActionResultWithApiResponse()
    {
        // Arrange
        const int projectId = 100;
        var projectDto = new ProjectUpdateDto
        {
            Name = "Response Project", Code = "RSP001",
            IsActive = true
        };

        var serviceResponse =
                ApiResponse<Project>.Success(null!, "Success");

        _projectService
                .UpdateProjectAsync(projectId,
                        Arg.Any<Project>())
                .Returns(serviceResponse);

        // Act
        var result =
                await _controller.UpdateProject(projectId,
                        projectDto);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
        Assert.IsInstanceOfType(result.Result,
                typeof(ObjectResult));

        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.IsInstanceOfType(objectResult.Value,
                typeof(ApiResponse<Project>));
    }

    /// <summary>
    ///     Тест: метод логирует обновление проекта с ID
    /// </summary>
    [TestMethod]
    public async Task
            UpdateProject_WithId_LogsInformationWithProjectId()
    {
        // Arrange
        const int projectId = 777;
        var projectDto = new ProjectUpdateDto
        {
            Name = "Log Test Project",
            Code = "LOG001",
            IsActive = true
        };

        _projectService
                .UpdateProjectAsync(projectId,
                        Arg.Any<Project>())
                .Returns(
                        ApiResponse<Project>.Success(null!,
                                "Success"));

        // Act
        await _controller.UpdateProject(projectId, projectDto);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Получен запрос на обновление проекта с ID:") &&
                                o.ToString()!.Contains(
                                        projectId.ToString())),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }
}