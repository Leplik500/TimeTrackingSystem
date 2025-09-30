using Microsoft.AspNetCore.Mvc;
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
    }
}