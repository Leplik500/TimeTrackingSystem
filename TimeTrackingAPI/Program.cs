using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TimeTrackingAPI.Data;
using TimeTrackingAPI.Services;
using TimeTrackingAPI.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Регистрация DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

// Регистрация сервисов
builder.Services.AddScoped<IProjectService, ProjectService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Time Tracking API",
        Version = "v1",
        Description = "API для системы учета рабочего времени",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Разработчик API",
            Email = "developer@timetracking.com"
        }
    });
    
    // Добавляем XML комментарии для Swagger
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    
    // Проверяем существование файла XML документации
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
        Console.WriteLine($"XML документация загружена из: {xmlPath}");
    }
    else
    {
        Console.WriteLine($"Файл XML документации не найден: {xmlPath}");
    }
    
    // Настройка отображения enum как строки
    options.UseInlineDefinitionsForEnums();
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

// Swagger доступен всегда
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Time Tracking API v1");
    options.RoutePrefix = string.Empty;
    options.DisplayRequestDuration();
    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
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
