using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TimeTrackingAPI.Common.DTOs;
using TimeTrackingAPI.Common.Enums;
using TimeTrackingAPI.Common.Models;
using TimeTrackingAPI.Data;
using TimeTrackingAPI.Models;
using TimeTrackingAPI.Services.Interfaces;

namespace TimeTrackingAPITests.Controllers.TimeEntriesController;

/// <summary>
///     Тесты для метода TimeEntriesController.CreateTimeEntry
/// </summary>
[TestClass]
public sealed class TimeEntriesControllerCreateTimeEntryTests
{
    private ApplicationDbContext _context = null!;

    private TimeTrackingAPI.Controllers.TimeEntriesController
            _controller = null!;

    private
            ILogger<TimeTrackingAPI.Controllers.TimeEntriesController> _logger = null!;

    private ITimeEntryService _timeEntryService = null!;

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
        _timeEntryService = Substitute.For<ITimeEntryService>();
        _logger = Substitute
                .For<ILogger<TimeTrackingAPI.Controllers.TimeEntriesController>>();

        _controller =
                new TimeTrackingAPI.Controllers.TimeEntriesController(_context,
                                _timeEntryService, _logger);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Dispose();
    }

    /// <summary>
    ///     Тест: метод возвращает BadRequest для несуществующей задачи
    /// </summary>
    [TestMethod]
    public async Task
            CreateTimeEntry_WithNonExistentTask_ReturnsBadRequest()
    {
        // Arrange
        var timeEntryDto = new TimeEntryCreateDto
        {
            Date = new DateTime(2025, 10, 15),
            Hours = 4,
            Description = "Test work",
            TaskId = 999 // Несуществующая задача
        };

        // Act
        var result =
                await _controller.CreateTimeEntry(timeEntryDto);

        // Assert
        var badRequestResult =
                result.Result as BadRequestObjectResult;

        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual((int) HttpStatusCode.BadRequest,
                badRequestResult.StatusCode);

        var apiResponse =
                badRequestResult.Value as ApiResponse<TimeEntry>;

        Assert.IsNotNull(apiResponse);
        Assert.AreEqual(ApiStatusCode.BadRequest,
                apiResponse.StatusCode);

        Assert.IsTrue(
                apiResponse.Message.Contains(
                        "Задача с ID 999 не найдена"));
    }

    /// <summary>
    ///     Тест: метод возвращает BadRequest для неактивной задачи
    /// </summary>
    [TestMethod]
    public async Task
            CreateTimeEntry_WithInactiveTask_ReturnsBadRequest()
    {
        // Arrange
        const int taskId = 1;
        var project = new Project
        {
            Id = 1,
            Name = "Test Project",
            Code = "TST",
            IsActive = true
        };

        var inactiveTask = new WorkTask
        {
            Id = taskId,
            Name = "Inactive Task",
            IsActive = false,
            ProjectId = 1,
            Project = project
        };

        await _context.Projects.AddAsync(project);
        await _context.WorkTasks.AddAsync(inactiveTask);
        await _context.SaveChangesAsync();

        var timeEntryDto = new TimeEntryCreateDto
        {
            Date = new DateTime(2025, 8, 20),
            Hours = 3,
            Description = "Work on inactive task",
            TaskId = taskId
        };

        // Act
        var result =
                await _controller.CreateTimeEntry(timeEntryDto);

        // Assert
        var badRequestResult =
                result.Result as BadRequestObjectResult;

        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual((int) HttpStatusCode.BadRequest,
                badRequestResult.StatusCode);

        var apiResponse =
                badRequestResult.Value as ApiResponse<TimeEntry>;

        Assert.IsNotNull(apiResponse);
        Assert.AreEqual(ApiStatusCode.BadRequest,
                apiResponse.StatusCode);

        Assert.IsTrue(
                apiResponse.Message.Contains(
                        "Задача 'Inactive Task' неактивна"));
    }

    /// <summary>
    ///     Тест: метод возвращает BadRequest при превышении дневного лимита часов
    /// </summary>
    [TestMethod]
    public async Task
            CreateTimeEntry_WithExceededDailyHours_ReturnsBadRequest()
    {
        // Arrange
        const int taskId = 2;
        var project = new Project
        {
            Id = 2,
            Name = "Hours Project",
            Code = "HRS",
            IsActive = true
        };

        var activeTask = new WorkTask
        {
            Id = taskId,
            Name = "Active Task",
            IsActive = true,
            ProjectId = 2,
            Project = project
        };

        await _context.Projects.AddAsync(project);
        await _context.WorkTasks.AddAsync(activeTask);
        await _context.SaveChangesAsync();

        var timeEntryDto = new TimeEntryCreateDto
        {
            Date = new DateTime(2025, 6, 10),
            Hours = 25, // Превышение 24 часов в день
            Description = "Too many hours",
            TaskId = taskId
        };

        // Act
        var result =
                await _controller.CreateTimeEntry(timeEntryDto);

        // Assert
        var badRequestResult =
                result.Result as BadRequestObjectResult;

        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual((int) HttpStatusCode.BadRequest,
                badRequestResult.StatusCode);

        var apiResponse =
                badRequestResult.Value as ApiResponse<TimeEntry>;

        Assert.IsNotNull(apiResponse);
        Assert.AreEqual(ApiStatusCode.BadRequest,
                apiResponse.StatusCode);
    }

    /// <summary>
    ///     Тест: метод создает TimeEntry из TimeEntryCreateDto с правильными
    ///     свойствами
    /// </summary>
    [TestMethod]
    public async Task
            CreateTimeEntry_WithValidDto_CreatesTimeEntryWithCorrectProperties()
    {
        // Arrange
        const int taskId = 3;
        var project = new Project
        {
            Id = 3,
            Name = "Valid Project",
            Code = "VLD",
            IsActive = true
        };

        var activeTask = new WorkTask
        {
            Id = taskId,
            Name = "Valid Task",
            IsActive = true,
            ProjectId = 3,
            Project = project
        };

        await _context.Projects.AddAsync(project);
        await _context.WorkTasks.AddAsync(activeTask);
        await _context.SaveChangesAsync();

        var timeEntryDto = new TimeEntryCreateDto
        {
            Date = new DateTime(2025, 4, 25, 14, 30,
                    45), // С временем
            Hours = 5.5m,
            Description = "Development work",
            TaskId = taskId
        };

        TimeEntry? capturedTimeEntry = null;
        _timeEntryService.CreateTimeEntryAsync(
                        Arg.Do<TimeEntry>(te =>
                                capturedTimeEntry = te))
                .Returns(ApiResponse<TimeEntry>.Success(
                        new TimeEntry {Task = activeTask},
                        "Success"));

        // Act
        await _controller.CreateTimeEntry(timeEntryDto);

        // Assert
        Assert.IsNotNull(capturedTimeEntry);
        Assert.AreEqual(5.5m, capturedTimeEntry.Hours);
        Assert.AreEqual("Development work",
                capturedTimeEntry.Description);

        Assert.AreEqual(taskId, capturedTimeEntry.TaskId);
        Assert.IsNull(capturedTimeEntry
                .Task); // null! в контроллере
    }

    /// <summary>
    ///     Тест: метод обнуляет время в дате при создании TimeEntry
    /// </summary>
    [TestMethod]
    public async Task
            CreateTimeEntry_WithDateTime_SetsTimeToZeroInTimeEntry()
    {
        // Arrange
        const int taskId = 4;
        var project = new Project
        {
            Id = 4,
            Name = "Time Project",
            Code = "TIM",
            IsActive = true
        };

        var activeTask = new WorkTask
        {
            Id = taskId,
            Name = "Time Test Task",
            IsActive = true,
            ProjectId = 4,
            Project = project
        };

        await _context.Projects.AddAsync(project);
        await _context.WorkTasks.AddAsync(activeTask);
        await _context.SaveChangesAsync();

        var dateWithTime =
                new DateTime(2025, 7, 12, 15, 45,
                        30); // 15:45:30

        var expectedDateOnly =
                new DateTime(2025, 7, 12, 0, 0, 0); // 00:00:00

        var timeEntryDto = new TimeEntryCreateDto
        {
            Date = dateWithTime,
            Hours = 2,
            Description = "Time normalization test",
            TaskId = taskId
        };

        TimeEntry? capturedTimeEntry = null;
        _timeEntryService.CreateTimeEntryAsync(
                        Arg.Do<TimeEntry>(te =>
                                capturedTimeEntry = te))
                .Returns(ApiResponse<TimeEntry>.Success(
                        new TimeEntry {Task = activeTask},
                        "Success"));

        // Act
        await _controller.CreateTimeEntry(timeEntryDto);

        // Assert
        Assert.IsNotNull(capturedTimeEntry);
        Assert.AreEqual(expectedDateOnly,
                capturedTimeEntry.Date);
    }

    /// <summary>
    ///     Тест: метод вызывает CreateTimeEntryAsync у сервиса с созданной проводкой
    /// </summary>
    [TestMethod]
    public async Task
            CreateTimeEntry_WithValidDto_CallsTimeEntryServiceCreateTimeEntryAsync()
    {
        // Arrange
        const int taskId = 5;
        var project = new Project
        {
            Id = 5,
            Name = "Service Project",
            Code = "SRV",
            IsActive = true
        };

        var activeTask = new WorkTask
        {
            Id = taskId,
            Name = "Service Call Task",
            IsActive = true,
            ProjectId = 5,
            Project = project
        };

        await _context.Projects.AddAsync(project);
        await _context.WorkTasks.AddAsync(activeTask);
        await _context.SaveChangesAsync();

        var timeEntryDto = new TimeEntryCreateDto
        {
            Date = new DateTime(2025, 9, 5),
            Hours = 6,
            Description = "Service test",
            TaskId = taskId
        };

        _timeEntryService
                .CreateTimeEntryAsync(Arg.Any<TimeEntry>())
                .Returns(ApiResponse<TimeEntry>.Success(
                        new TimeEntry {Task = activeTask},
                        "Success"));

        // Act
        await _controller.CreateTimeEntry(timeEntryDto);

        // Assert
        await _timeEntryService.Received(1)
                .CreateTimeEntryAsync(Arg.Any<TimeEntry>());
    }

    /// <summary>
    ///     Тест: метод возвращает CreatedAtAction для успешного создания
    /// </summary>
    [TestMethod]
    public async Task
            CreateTimeEntry_ServiceReturnsSuccess_ReturnsCreatedAtAction()
    {
        // Arrange
        const int taskId = 6;
        var project = new Project
        {
            Id = 6,
            Name = "Created Project",
            Code = "CRT",
            IsActive = true
        };

        var activeTask = new WorkTask
        {
            Id = taskId,
            Name = "Created Task",
            IsActive = true,
            ProjectId = 6,
            Project = project
        };

        await _context.Projects.AddAsync(project);
        await _context.WorkTasks.AddAsync(activeTask);
        await _context.SaveChangesAsync();

        var timeEntryDto = new TimeEntryCreateDto
        {
            Date = new DateTime(2025, 11, 8),
            Hours = 7,
            Description = "Created entry",
            TaskId = taskId
        };

        var createdTimeEntry = new TimeEntry
        {
            Id = 123,
            Date = new DateTime(2025, 11, 8),
            Hours = 7,
            Description = "Created entry",
            TaskId = taskId,
            Task = activeTask
        };

        var serviceResponse =
                ApiResponse<TimeEntry>.Success(createdTimeEntry,
                        "Time entry created");

        _timeEntryService
                .CreateTimeEntryAsync(Arg.Any<TimeEntry>())
                .Returns(serviceResponse);

        // Act
        var result =
                await _controller.CreateTimeEntry(timeEntryDto);

        // Assert
        Assert.IsInstanceOfType(result.Result,
                typeof(CreatedAtActionResult));
    }

    /// <summary>
    ///     Тест: метод возвращает правильный action name для CreatedAtAction
    /// </summary>
    [TestMethod]
    public async Task
            CreateTimeEntry_ServiceReturnsSuccess_ReturnsCreatedAtActionWithGetTimeEntriesAction()
    {
        // Arrange
        const int taskId = 7;
        var project = new Project
        {
            Id = 7,
            Name = "Action Project",
            Code = "ACT",
            IsActive = true
        };

        var activeTask = new WorkTask
        {
            Id = taskId,
            Name = "Action Test Task",
            IsActive = true,
            ProjectId = 7,
            Project = project
        };

        await _context.Projects.AddAsync(project);
        await _context.WorkTasks.AddAsync(activeTask);
        await _context.SaveChangesAsync();

        var timeEntryDto = new TimeEntryCreateDto
        {
            Date = new DateTime(2025, 12, 20),
            Hours = 1.5m,
            Description = "Action test",
            TaskId = taskId
        };

        var createdTimeEntry = new TimeEntry
                {Id = 456, Task = activeTask};

        var serviceResponse =
                ApiResponse<TimeEntry>.Success(createdTimeEntry,
                        "Success");

        _timeEntryService
                .CreateTimeEntryAsync(Arg.Any<TimeEntry>())
                .Returns(serviceResponse);

        // Act
        var result =
                await _controller.CreateTimeEntry(timeEntryDto);

        // Assert
        var createdResult =
                result.Result as CreatedAtActionResult;

        Assert.IsNotNull(createdResult);
        Assert.AreEqual("GetTimeEntries",
                createdResult.ActionName);

        Assert.AreSame(serviceResponse, createdResult.Value);
    }

    /// <summary>
    ///     Тест: метод возвращает StatusCode BadRequest для ошибки валидации сервиса
    /// </summary>
    [TestMethod]
    public async Task
            CreateTimeEntry_ServiceReturnsBadRequest_ReturnsStatusCode400()
    {
        // Arrange
        const int taskId = 8;
        var project = new Project
        {
            Id = 8,
            Name = "BadReq Project",
            Code = "BAD",
            IsActive = true
        };

        var activeTask = new WorkTask
        {
            Id = taskId,
            Name = "Bad Request Task",
            IsActive = true,
            ProjectId = 8,
            Project = project
        };

        await _context.Projects.AddAsync(project);
        await _context.WorkTasks.AddAsync(activeTask);
        await _context.SaveChangesAsync();

        var timeEntryDto = new TimeEntryCreateDto
        {
            Date = new DateTime(2025, 3, 15),
            Hours = 8,
            Description = "Service validation error",
            TaskId = taskId
        };

        var serviceResponse = ApiResponse<TimeEntry>.Error(
                ApiStatusCode.BadRequest,
                "Service validation failed");

        _timeEntryService
                .CreateTimeEntryAsync(Arg.Any<TimeEntry>())
                .Returns(serviceResponse);

        // Act
        var result =
                await _controller.CreateTimeEntry(timeEntryDto);

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
            CreateTimeEntry_ServiceReturnsInternalServerError_ReturnsStatusCode500()
    {
        // Arrange
        const int taskId = 9;
        var project = new Project
        {
            Id = 9,
            Name = "Error Project",
            Code = "ERR",
            IsActive = true
        };

        var activeTask = new WorkTask
        {
            Id = taskId,
            Name = "Server Error Task",
            IsActive = true,
            ProjectId = 9,
            Project = project
        };

        await _context.Projects.AddAsync(project);
        await _context.WorkTasks.AddAsync(activeTask);
        await _context.SaveChangesAsync();

        var timeEntryDto = new TimeEntryCreateDto
        {
            Date = new DateTime(2025, 1, 30),
            Hours = 4.5m,
            Description = "Server error test",
            TaskId = taskId
        };

        var serviceResponse = ApiResponse<TimeEntry>.Error(
                ApiStatusCode.InternalServerError,
                "Database connection failed");

        _timeEntryService
                .CreateTimeEntryAsync(Arg.Any<TimeEntry>())
                .Returns(serviceResponse);

        // Act
        var result =
                await _controller.CreateTimeEntry(timeEntryDto);

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
            CreateTimeEntry_ServiceResponse_ReturnsActionResultWithApiResponse()
    {
        // Arrange
        const int taskId = 10;
        var project = new Project
        {
            Id = 10,
            Name = "Response Project",
            Code = "RSP",
            IsActive = true
        };

        var activeTask = new WorkTask
        {
            Id = taskId,
            Name = "Response Type Task",
            IsActive = true,
            ProjectId = 10,
            Project = project
        };

        await _context.Projects.AddAsync(project);
        await _context.WorkTasks.AddAsync(activeTask);
        await _context.SaveChangesAsync();

        var timeEntryDto = new TimeEntryCreateDto
        {
            Date = new DateTime(2025, 5, 18),
            Hours = 3.5m,
            Description = "Response test",
            TaskId = taskId
        };

        var createdTimeEntry = new TimeEntry
                {Id = 789, Task = activeTask};

        var serviceResponse =
                ApiResponse<TimeEntry>.Success(createdTimeEntry,
                        "Success");

        _timeEntryService
                .CreateTimeEntryAsync(Arg.Any<TimeEntry>())
                .Returns(serviceResponse);

        // Act
        var result =
                await _controller.CreateTimeEntry(timeEntryDto);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);

        // Для Success это CreatedAtActionResult
        var createdResult =
                result.Result as CreatedAtActionResult;

        Assert.IsNotNull(createdResult);
        Assert.IsInstanceOfType(createdResult.Value,
                typeof(ApiResponse<TimeEntry>));
    }

    /// <summary>
    ///     Тест: метод логирует создание проводки с датой и TaskId
    /// </summary>
    [TestMethod]
    public async Task
            CreateTimeEntry_WithDto_LogsInformationWithDateAndTaskId()
    {
        // Arrange
        const int taskId = 11;
        var project = new Project
        {
            Id = 11,
            Name = "Log Project",
            Code = "LOG",
            IsActive = true
        };

        var activeTask = new WorkTask
        {
            Id = taskId,
            Name = "Logging Task",
            IsActive = true,
            ProjectId = 11,
            Project = project
        };

        await _context.Projects.AddAsync(project);
        await _context.WorkTasks.AddAsync(activeTask);
        await _context.SaveChangesAsync();

        var testDate = new DateTime(2025, 8, 14, 16, 20, 30);
        var expectedFormattedDate = "2025-08-14"; // yyyy-MM-dd

        var timeEntryDto = new TimeEntryCreateDto
        {
            Date = testDate,
            Hours = 6.5m,
            Description = "Logging test work",
            TaskId = taskId
        };

        _timeEntryService
                .CreateTimeEntryAsync(Arg.Any<TimeEntry>())
                .Returns(ApiResponse<TimeEntry>.Success(
                        new TimeEntry {Task = activeTask},
                        "Success"));

        // Act
        await _controller.CreateTimeEntry(timeEntryDto);

        // Assert
        _logger.Received(1)
                .Log(
                        LogLevel.Information,
                        Arg.Any<EventId>(),
                        Arg.Is<object>(o =>
                                o.ToString()!.Contains(
                                        "Получен запрос на создание проводки времени на дату") &&
                                o.ToString()!.Contains(
                                        expectedFormattedDate) &&
                                o.ToString()!.Contains(
                                        $"для задачи {taskId}")),
                        Arg.Any<Exception>(),
                        Arg.Any<Func<object, Exception?,
                                string>>());
    }

    /// <summary>
    ///     Тест: метод не вызывает сервис для несуществующей задачи
    /// </summary>
    [TestMethod]
    public async Task
            CreateTimeEntry_WithNonExistentTask_DoesNotCallService()
    {
        // Arrange
        var timeEntryDto = new TimeEntryCreateDto
        {
            Date = new DateTime(2025, 2, 10),
            Hours = 2,
            Description = "Non-existent task test",
            TaskId = 888 // Несуществующая задача
        };

        // Act
        await _controller.CreateTimeEntry(timeEntryDto);

        // Assert
        await _timeEntryService.DidNotReceive()
                .CreateTimeEntryAsync(Arg.Any<TimeEntry>());
    }

    /// <summary>
    ///     Тест: метод не вызывает сервис для неактивной задачи
    /// </summary>
    [TestMethod]
    public async Task
            CreateTimeEntry_WithInactiveTask_DoesNotCallService()
    {
        // Arrange
        const int taskId = 12;
        var project = new Project
        {
            Id = 12,
            Name = "NoService Project",
            Code = "NSR",
            IsActive = true
        };

        var inactiveTask = new WorkTask
        {
            Id = taskId,
            Name = "Inactive No Service Task",
            IsActive = false,
            ProjectId = 12,
            Project = project
        };

        await _context.Projects.AddAsync(project);
        await _context.WorkTasks.AddAsync(inactiveTask);
        await _context.SaveChangesAsync();

        var timeEntryDto = new TimeEntryCreateDto
        {
            Date = new DateTime(2025, 10, 25),
            Hours = 1,
            Description = "Inactive task no service call",
            TaskId = taskId
        };

        // Act
        await _controller.CreateTimeEntry(timeEntryDto);

        // Assert
        await _timeEntryService.DidNotReceive()
                .CreateTimeEntryAsync(Arg.Any<TimeEntry>());
    }

    /// <summary>
    ///     Тест: метод не вызывает сервис при превышении дневного лимита
    /// </summary>
    [TestMethod]
    public async Task
            CreateTimeEntry_WithExceededDailyHours_DoesNotCallService()
    {
        // Arrange
        const int taskId = 13;
        var project = new Project
        {
            Id = 13,
            Name = "Exceed Project",
            Code = "EXC",
            IsActive = true
        };

        var activeTask = new WorkTask
        {
            Id = taskId,
            Name = "Exceeded Hours Task",
            IsActive = true,
            ProjectId = 13,
            Project = project
        };

        await _context.Projects.AddAsync(project);
        await _context.WorkTasks.AddAsync(activeTask);
        await _context.SaveChangesAsync();

        var timeEntryDto = new TimeEntryCreateDto
        {
            Date = new DateTime(2025, 6, 30),
            Hours = 30, // Превышение лимита
            Description = "Exceeded daily hours test",
            TaskId = taskId
        };

        // Act
        await _controller.CreateTimeEntry(timeEntryDto);

        // Assert
        await _timeEntryService.DidNotReceive()
                .CreateTimeEntryAsync(Arg.Any<TimeEntry>());
    }
}