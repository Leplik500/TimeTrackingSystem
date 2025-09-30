using Microsoft.AspNetCore.Mvc;
using TimeTrackingAPI.Common.DTOs;
using TimeTrackingAPI.Common.Models;
using TimeTrackingAPI.Models;
using TimeTrackingAPI.Services.Interfaces;

namespace TimeTrackingAPI.Controllers
{
    /// <summary>
    /// Контроллер для работы с проектами
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public sealed class ProjectsController(
        IProjectService projectService,
        ILogger<ProjectsController> logger) : ControllerBase
    {
        /// <summary>
        /// Получить все проекты
        /// </summary>
        /// <returns>Список всех проектов компании</returns>
        /// <response code="200">Список проектов успешно получен</response>
        /// <response code="500">Внутренняя ошибка сервера</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<Project>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<List<Project>>), 500)]
        public async Task<ActionResult<ApiResponse<List<Project>>>> GetProjects()
        {
            logger.LogInformation("Получен запрос на получение всех проектов");
            
            var response = await projectService.GetAllProjectsAsync();
            
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Получить проект по ID
        /// </summary>
        /// <param name="id">Идентификатор проекта</param>
        /// <returns>Найденный проект или сообщение об ошибке</returns>
        /// <response code="200">Проект найден</response>
        /// <response code="404">Проект не найден</response>
        /// <response code="500">Внутренняя ошибка сервера</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<Project>), 200)]
        [ProducesResponseType(typeof(ApiResponse<Project>), 404)]
        [ProducesResponseType(typeof(ApiResponse<Project>), 500)]
        public async Task<ActionResult<ApiResponse<Project>>> GetProject(int id)
        {
            logger.LogInformation("Получен запрос на получение проекта с ID: {Id}", id);
            
            var response = await projectService.GetProjectByIdAsync(id);
            
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Создать новый проект
        /// </summary>
        /// <param name="projectDto">Данные для создания проекта</param>
        /// <returns>Созданный проект</returns>
        /// <response code="201">Проект успешно создан</response>
        /// <response code="400">Некорректные данные запроса или проект с таким кодом уже существует</response>
        /// <response code="500">Внутренняя ошибка сервера</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<Project>), 201)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ApiResponse<Project>), 500)]
        public async Task<ActionResult<ApiResponse<Project>>> CreateProject(ProjectCreateDto projectDto)
        {
            logger.LogInformation("Получен запрос на создание проекта: {Name}", projectDto.Name);
            
            // ModelState.IsValid проверка АВТОМАТИЧЕСКАЯ благодаря [ApiController]!
            // Если данные невалидны, до сюда код не дойдет - автоматически вернется 400
            
            var project = new Project
            {
                Name = projectDto.Name,
                Code = projectDto.Code,
                IsActive = projectDto.IsActive
            };

            var response = await projectService.CreateProjectAsync(project);
            
            if (response.IsSuccess)
            {
                return CreatedAtAction(
                    nameof(GetProject), 
                    new { id = response.Data!.Id }, 
                    response);
            }
            
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Обновить существующий проект
        /// </summary>
        /// <param name="id">Идентификатор проекта</param>
        /// <param name="projectDto">Обновленные данные проекта</param>
        /// <returns>Результат обновления</returns>
        /// <response code="200">Проект успешно обновлен</response>
        /// <response code="400">Некорректные данные запроса</response>
        /// <response code="404">Проект не найден</response>
        /// <response code="500">Внутренняя ошибка сервера</response>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<Project>), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ApiResponse<Project>), 404)]
        [ProducesResponseType(typeof(ApiResponse<Project>), 500)]
        public async Task<ActionResult<ApiResponse<Project>>> UpdateProject(int id, ProjectUpdateDto projectDto)
        {
            logger.LogInformation("Получен запрос на обновление проекта с ID: {Id}", id);
            
            var project = new Project
            {
                Id = id,
                Name = projectDto.Name,
                Code = projectDto.Code,
                IsActive = projectDto.IsActive
            };

            var response = await projectService.UpdateProjectAsync(id, project);
            
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Удалить проект
        /// </summary>
        /// <param name="id">Идентификатор проекта</param>
        /// <returns>Результат удаления</returns>
        /// <response code="200">Проект успешно удален</response>
        /// <response code="400">Проект нельзя удалить из-за наличия связанных задач</response>
        /// <response code="404">Проект не найден</response>
        /// <response code="500">Внутренняя ошибка сервера</response>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 500)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteProject(int id)
        {
            logger.LogInformation("Получен запрос на удаление проекта с ID: {Id}", id);
            
            var response = await projectService.DeleteProjectAsync(id);
            
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
