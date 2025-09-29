using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeTrackingAPI.Data;
using TimeTrackingAPI.Models;

namespace TimeTrackingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController(
            ApplicationDbContext context,
            ILogger<TestController> logger)
            : ControllerBase
    {
        /// <summary>
        /// Проверка работоспособности API
        /// </summary>
        /// <returns>Статус API</returns>
        [HttpGet("status")]
        public async Task<ActionResult> GetStatus()
        {
            try
            {
                // Проверяем подключение к БД
                var canConnect = await context.Database.CanConnectAsync();
                
                return Ok(new
                {
                    Status = "OK",
                    Timestamp = DateTime.UtcNow,
                    DatabaseConnection = canConnect ? "Connected" : "Disconnected",
                    Message = "Time Tracking API is working!"
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking API status");
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Получить информацию о базе данных
        /// </summary>
        /// <returns>Информация о БД</returns>
        [HttpGet("database-info")]
        public async Task<ActionResult> GetDatabaseInfo()
        {
            try
            {
                var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                
                return Ok(new
                {
                    AppliedMigrations = appliedMigrations.ToArray(),
                    PendingMigrations = pendingMigrations.ToArray(),
                    DatabaseExists = await context.Database.CanConnectAsync()
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting database info");
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
