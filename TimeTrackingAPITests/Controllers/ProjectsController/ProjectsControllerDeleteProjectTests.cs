using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TimeTrackingAPI.Common.Enums;
using TimeTrackingAPI.Common.Models;
using TimeTrackingAPI.Services.Interfaces;

namespace TimeTrackingAPITests.Controllers.ProjectsController;

/// <summary>
///     Тесты для метода ProjectsController.DeleteProject
/// </summary>
[TestClass]
public sealed class ProjectsControllerDeleteProjectTests
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
    ///     Тест: метод вызывает DeleteProjectAsync у сервиса с переданным ID
    /// </summary>
    [TestMethod]
    public async Task
            DeleteProject_WithId_CallsProjectServiceDeleteProjectAsyncWithSameId()
    {
        // Arrange
        const int projectId = 42;
        var serviceResponse =
                ApiResponse<bool>.Success(true,
                        "Project deleted");

        _projectService.DeleteProjectAsync(projectId)
                .Returns(serviceResponse);

        // Act
        await _controller.DeleteProject(projectId);

        // Assert
        await _projectService.Received(1)
                .DeleteProjectAsync(projectId);
    }

    /// <summary>
    ///     Тест: метод возвращает результат сервиса без изменений
    /// </summary>
    [TestMethod]
    public async Task
            DeleteProject_ServiceReturnsData_ReturnsServiceResponseUnchanged()
    {
        // Arrange
        const int projectId = 15;
        var serviceResponse =
                ApiResponse<bool>.Success(true,
                        "Проект удален успешно");

        _projectService.DeleteProjectAsync(projectId)
                .Returns(serviceResponse);

        // Act
        var result = await _controller.DeleteProject(projectId);

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        var apiResponse =
                objectResult.Value as ApiResponse<bool>;

        Assert.IsNotNull(apiResponse);

        Assert.AreSame(serviceResponse, apiResponse);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode Success для успешного удаления
    /// </summary>
    [TestMethod]
    public async Task
            DeleteProject_ServiceReturnsSuccess_ReturnsStatusCode200()
    {
        // Arrange
        const int projectId = 100;
        var serviceResponse =
                ApiResponse<bool>.Success(true,
                        "Success message");

        _projectService.DeleteProjectAsync(projectId)
                .Returns(serviceResponse);

        // Act
        var result = await _controller.DeleteProject(projectId);

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
            DeleteProject_ServiceReturnsNotFound_ReturnsStatusCode404()
    {
        // Arrange
        const int projectId = 999;
        var serviceResponse = ApiResponse<bool>.Error(
                ApiStatusCode.NotFound,
                "Project not found");

        _projectService.DeleteProjectAsync(projectId)
                .Returns(serviceResponse);

        // Act
        var result = await _controller.DeleteProject(projectId);

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual((int) HttpStatusCode.NotFound,
                objectResult.StatusCode);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode BadRequest для проекта со связанными
    ///     задачами
    /// </summary>
    [TestMethod]
    public async Task
            DeleteProject_ServiceReturnsBadRequest_ReturnsStatusCode400()
    {
        // Arrange
        const int projectId = 50;
        var serviceResponse = ApiResponse<bool>.Error(
                ApiStatusCode.BadRequest,
                "Cannot delete project with related tasks");

        _projectService.DeleteProjectAsync(projectId)
                .Returns(serviceResponse);

        // Act
        var result = await _controller.DeleteProject(projectId);

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
            DeleteProject_ServiceReturnsInternalServerError_ReturnsStatusCode500()
    {
        // Arrange
        const int projectId = 75;
        var serviceResponse = ApiResponse<bool>.Error(
                ApiStatusCode.InternalServerError,
                "Internal error");

        _projectService.DeleteProjectAsync(projectId)
                .Returns(serviceResponse);

        // Act
        var result = await _controller.DeleteProject(projectId);

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
            DeleteProject_ServiceResponse_ReturnsActionResultWithApiResponse()
    {
        // Arrange
        const int projectId = 25;
        var serviceResponse =
                ApiResponse<bool>.Success(true, "Test message");

        _projectService.DeleteProjectAsync(projectId)
                .Returns(serviceResponse);

        // Act
        var result = await _controller.DeleteProject(projectId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
        Assert.IsInstanceOfType(result.Result,
                typeof(ObjectResult));

        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.IsInstanceOfType(objectResult.Value,
                typeof(ApiResponse<bool>));
    }

    /// <summary>
    ///     Тест: метод логирует получение запроса с ID проекта
    /// </summary>
    [TestMethod]
    public async Task
            DeleteProject_InvokedWithId_LogsInformationWithProjectId()
    {
        // Arrange
        const int projectId = 123;
        var serviceResponse =
                ApiResponse<bool>.Success(true, "Test message");

        _projectService.DeleteProjectAsync(projectId)
                .Returns(serviceResponse);

        // Act
        await _controller.DeleteProject(projectId);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Получен запрос на удаление проекта с ID:") &&
                                o.ToString()!.Contains(
                                        projectId.ToString())),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }
}