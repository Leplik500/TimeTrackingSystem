using Microsoft.EntityFrameworkCore;
using TimeTrackingAPI.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Регистрация DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Time Tracking API",
        Version = "v1",
        Description = "API для системы учета рабочего времени"
    });
});

var app = builder.Build();

// Автоматическое применение миграций при запуске
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        var migrations = pendingMigrations as string[] ?? pendingMigrations.ToArray();
        if (migrations.Length != 0)
        {
            logger.LogInformation("Applying {Count} pending migrations: {Migrations}",
                migrations.Length, string.Join(", ", migrations));
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully");
        }
        else
        {
            logger.LogInformation("No pending migrations found. Database is up to date.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error occurred while applying database migrations");
        throw;
    }
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Time Tracking API v1");
    options.RoutePrefix = string.Empty;
});


app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => new { 
    Status = "Healthy", 
    Timestamp = DateTime.UtcNow,
    Message = "Time Tracking API is running!" 
});

app.MapGet("/api", () => Results.Redirect("/swagger"));

app.Run();
