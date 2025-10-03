using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TimeTrackingAPI.Common.Enums;
using TimeTrackingAPI.Common.Models;
using TimeTrackingAPI.Models;
using TimeTrackingAPI.Services.Interfaces;

namespace TimeTrackingAPITests.Controllers.ProjectsController;

/// <summary>
///     Тесты для метода ProjectsController.GetProject
/// </summary>
[TestClass]
public sealed class ProjectsControllerGetProjectTests
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
    ///     Тест: метод вызывает GetProjectByIdAsync у сервиса с переданным ID
    /// </summary>
    [TestMethod]
    public async Task
            GetProject_WithId_CallsProjectServiceGetProjectByIdAsyncWithSameId()
    {
        // Arrange
        const int projectId = 42;
        var serviceResponse =
                ApiResponse<Project>.Success(null!,
                        "Test message");

        _projectService.GetProjectByIdAsync(projectId)
                .Returns(serviceResponse);

        // Act
        await _controller.GetProject(projectId);

        // Assert
        await _projectService.Received(1)
                .GetProjectByIdAsync(projectId);
    }

    /// <summary>
    ///     Тест: метод возвращает результат сервиса без изменений
    /// </summary>
    [TestMethod]
    public async Task
            GetProject_ServiceReturnsData_ReturnsServiceResponseUnchanged()
    {
        // Arrange
        const int projectId = 15;
        var serviceResponse =
                ApiResponse<Project>.Success(null!,
                        "Проект найден");

        _projectService.GetProjectByIdAsync(projectId)
                .Returns(serviceResponse);

        // Act
        var result = await _controller.GetProject(projectId);

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        var apiResponse =
                objectResult.Value as ApiResponse<Project>;

        Assert.IsNotNull(apiResponse);

        Assert.AreSame(serviceResponse, apiResponse);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode Success для найденного проекта
    /// </summary>
    [TestMethod]
    public async Task
            GetProject_ServiceReturnsSuccess_ReturnsStatusCode200()
    {
        // Arrange
        const int projectId = 100;
        var serviceResponse =
                ApiResponse<Project>.Success(null!,
                        "Success message");

        _projectService.GetProjectByIdAsync(projectId)
                .Returns(serviceResponse);

        // Act
        var result = await _controller.GetProject(projectId);

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
            GetProject_ServiceReturnsNotFound_ReturnsStatusCode404()
    {
        // Arrange
        const int projectId = 999;
        var serviceResponse = ApiResponse<Project>.Error(
                ApiStatusCode.NotFound,
                "Project not found");

        _projectService.GetProjectByIdAsync(projectId)
                .Returns(serviceResponse);

        // Act
        var result = await _controller.GetProject(projectId);

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual((int) HttpStatusCode.NotFound,
                objectResult.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode InternalServerError для ошибки сервиса
    /// </summary>
    [TestMethod]
    public async Task
            GetProject_ServiceReturnsInternalServerError_ReturnsStatusCode500()
    {
        // Arrange
        const int projectId = 50;
        var serviceResponse = ApiResponse<Project>.Error(
                ApiStatusCode.InternalServerError,
                "Internal error");

        _projectService.GetProjectByIdAsync(projectId)
                .Returns(serviceResponse);

        // Act
        var result = await _controller.GetProject(projectId);

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
            GetProject_ServiceResponse_ReturnsActionResultWithApiResponse()
    {
        // Arrange
        const int projectId = 25;
        var serviceResponse =
                ApiResponse<Project>.Success(null!,
                        "Test message");

        _projectService.GetProjectByIdAsync(projectId)
                .Returns(serviceResponse);

        // Act
        var result = await _controller.GetProject(projectId);

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
    ///     Тест: метод логирует получение запроса с ID проекта
    /// </summary>
    [TestMethod]
    public async Task
            GetProject_InvokedWithId_LogsInformationWithProjectId()
    {
        // Arrange
        const int projectId = 123;
        var serviceResponse =
                ApiResponse<Project>.Success(null!,
                        "Test message");

        _projectService.GetProjectByIdAsync(projectId)
                .Returns(serviceResponse);

        // Act
        await _controller.GetProject(projectId);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Получен запрос на получение проекта с ID:") &&
                                o.ToString()!.Contains(
                                        projectId.ToString())),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }
}