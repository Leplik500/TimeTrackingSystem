using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TimeTrackingAPI.Models;

namespace TimeTrackingAPI.Data;

/// <summary>
///     Контекст базы данных
/// </summary>
public sealed class ApplicationDbContext : DbContext
{
    /// <summary>
    ///     Инициализирует новый экземпляр ApplicationDbContext
    /// </summary>
    /// <param name="options">Параметры контекста</param>
    public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options) :
            base(options)
    {
        // Отключаем отслеживание для запросов только для чтения
        ChangeTracker.QueryTrackingBehavior =
                QueryTrackingBehavior.NoTracking;
    }

    /// <summary>
    ///     Набор проектов в базе данных
    /// </summary>
    public DbSet<Project> Projects { get; set; } = null!;

    /// <summary>
    ///     Набор задач в базе данных
    /// </summary>
    public DbSet<WorkTask> WorkTasks { get; set; } = null!;

    /// <summary>
    ///     Набор проводок времени в базе данных
    /// </summary>
    public DbSet<TimeEntry> TimeEntries { get; set; } = null!;

    /// <summary>
    ///     Настройка модели данных при создании
    /// </summary>
    /// <param name="modelBuilder">Построитель модели</param>
    protected override void OnModelCreating(
            ModelBuilder modelBuilder)
    {
        // Применяем конфигурации из сборки
        modelBuilder.ApplyConfigurationsFromAssembly(
                Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}