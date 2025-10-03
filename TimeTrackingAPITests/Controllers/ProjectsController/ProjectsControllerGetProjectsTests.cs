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
///     Тесты для метода ProjectsController.GetProjects
/// </summary>
[TestClass]
public sealed class ProjectsControllerGetProjectsTests
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
    ///     Тест: метод вызывает GetAllProjectsAsync у сервиса
    /// </summary>
    [TestMethod]
    public async Task
            GetProjects_Invoked_CallsProjectServiceGetAllProjectsAsync()
    {
        // Arrange
        var serviceResponse = ApiResponse<List<Project>>.Success(
                [],
                "Test message");

        _projectService.GetAllProjectsAsync()
                .Returns(serviceResponse);

        // Act
        await _controller.GetProjects();

        // Assert
        await _projectService.Received(1).GetAllProjectsAsync();
    }

    /// <summary>
    ///     Тест: метод возвращает результат сервиса без изменений
    /// </summary>
    [TestMethod]
    public async Task
            GetProjects_ServiceReturnsData_ReturnsServiceResponseUnchanged()
    {
        // Arrange
        var serviceResponse = ApiResponse<List<Project>>.Success(
                [],
                "Получено 3 проекта");

        _projectService.GetAllProjectsAsync()
                .Returns(serviceResponse);

        // Act
        var result = await _controller.GetProjects();

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        var apiResponse =
                objectResult.Value as ApiResponse<List<Project>>;

        Assert.IsNotNull(apiResponse);

        Assert.AreSame(serviceResponse, apiResponse);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode Success для успешного ответа сервиса
    /// </summary>
    [TestMethod]
    public async Task
            GetProjects_ServiceReturnsSuccess_ReturnsStatusCode200()
    {
        // Arrange
        var serviceResponse = ApiResponse<List<Project>>.Success(
                [],
                "Success message");

        _projectService.GetAllProjectsAsync()
                .Returns(serviceResponse);

        // Act
        var result = await _controller.GetProjects();

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual((int) HttpStatusCode.OK,
                objectResult.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode InternalServerError для ошибки сервиса
    /// </summary>
    [TestMethod]
    public async Task
            GetProjects_ServiceReturnsInternalServerError_ReturnsStatusCode500()
    {
        // Arrange
        var serviceResponse = ApiResponse<List<Project>>.Error(
                ApiStatusCode.InternalServerError,
                "Internal error");

        _projectService.GetAllProjectsAsync()
                .Returns(serviceResponse);

        // Act
        var result = await _controller.GetProjects();

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
            GetProjects_ServiceResponse_ReturnsActionResultWithApiResponse()
    {
        // Arrange
        var serviceResponse = ApiResponse<List<Project>>.Success(
                [],
                "Test message");

        _projectService.GetAllProjectsAsync()
                .Returns(serviceResponse);

        // Act
        var result = await _controller.GetProjects();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
        Assert.IsInstanceOfType(result.Result,
                typeof(ObjectResult));

        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.IsInstanceOfType(objectResult.Value,
                typeof(ApiResponse<List<Project>>));
    }

    /// <summary>
    ///     Тест: метод логирует получение запроса на получение всех проектов
    /// </summary>
    [TestMethod]
    public async Task
            GetProjects_Invoked_LogsInformationAboutRequest()
    {
        // Arrange
        var serviceResponse = ApiResponse<List<Project>>.Success(
                [],
                "Test message");

        _projectService.GetAllProjectsAsync()
                .Returns(serviceResponse);

        // Act
        await _controller.GetProjects();

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Получен запрос на получение всех проектов")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }
}