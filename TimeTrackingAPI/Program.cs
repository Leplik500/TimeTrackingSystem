using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using Swashbuckle.AspNetCore.SwaggerUI;
using TimeTrackingAPI.Data;
using TimeTrackingAPI.Services;
using TimeTrackingAPI.Services.Interfaces;
using TimeTrackingAPI.Swagger;

var logger = LogManager.Setup()
        .LoadConfigurationFromAppSettings()
        .GetCurrentClassLogger();

try
{
    logger.Info("Starting Time Tracking API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    builder.Services.AddControllers();

    // Регистрация DbContext
    builder.Services
            .AddDbContext<ApplicationDbContext>(options =>
            {
                var connectionString =
                        builder.Configuration
                                .GetConnectionString(
                                        "DefaultConnection");

                options.UseSqlServer(connectionString);
            });

    // Регистрация сервисов
    builder.Services
            .AddScoped<IProjectService, ProjectService>();

    builder.Services.AddScoped<ITaskService, TaskService>();
    builder.Services
            .AddScoped<ITimeEntryService, TimeEntryService>();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Time Tracking API",
            Version = "v1",
            Description =
                    "API для системы учета рабочего времени",
            Contact = new OpenApiContact
            {
                Name = "Разработчик API",
                Email = "developer@timetracking.com"
            }
        });

        // Добавляем XML комментарии для Swagger
        var xmlFile =
                $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

        var xmlPath =
                Path.Combine(AppContext.BaseDirectory, xmlFile);

        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
            logger.Info(
                    "XML документация загружена из: {XmlPath}",
                    xmlPath);
        }
        else
        {
            logger.Warn(
                    "Файл XML документации не найден: {XmlPath}",
                    xmlPath);
        }

        options.UseInlineDefinitionsForEnums();
        options
                .OperationFilter<
                        TasksControllerSwaggerExamplesFilter>();

        options
                .OperationFilter<
                        TimeEntriesControllerSwaggerExamplesFilter>();

        options
                .OperationFilter<
                        ProjectsControllerSwaggerExamplesFilter>();
    });

    var app = builder.Build();

    // Автоматическое применение миграций при запуске
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        var migrationLogger = scope.ServiceProvider
                .GetRequiredService<ILogger<Program>>();

        try
        {
                migrationLogger.LogInformation(
                                "Проверка подключения к серверу баз данных...");

                await context.Database.EnsureCreatedAsync();
                migrationLogger.LogInformation(
                                "База данных готова");

            var pendingMigrations = await context.Database
                    .GetPendingMigrationsAsync();

            var migrations = pendingMigrations as string[] ??
                             pendingMigrations.ToArray();

            if (migrations.Length != 0)
            {
                migrationLogger.LogInformation(
                                "Применение {Count} ожидающих миграций: {Migrations}",
                        migrations.Length,
                        string.Join(", ", migrations));

                await context.Database.MigrateAsync();
                migrationLogger.LogInformation(
                                "Миграции базы данных успешно применены");
            }
            else
            {
                migrationLogger.LogInformation(
                                "Ожидающих миграций не найдено. База данных актуальна");
            }

            var canConnect =
                            await context.Database
                                            .CanConnectAsync();

            migrationLogger.LogInformation(
                            "Тест подключения к БД: {CanConnect}",
                            canConnect);
        }
        catch (Exception ex)
        {
            migrationLogger.LogError(ex,
                            "Ошибка при работе с базой данных");

            if (!builder.Environment.IsDevelopment())
                    throw;
        }
    }

    // Swagger доступен всегда
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json",
                "Time Tracking API v1");

        options.RoutePrefix = string.Empty;
        options.DisplayRequestDuration();
        options.DocExpansion(DocExpansion.List);
    });

    app.UseRouting();
    app.UseAuthorization();
    app.MapControllers();

    app.MapGet("/health", () => new
    {
        Status = "Healthy",
        Timestamp = DateTime.UtcNow,
        Message = "Time Tracking API is running!"
    });

    logger.Info("Time Tracking API configured successfully");
    await app.RunAsync();
}
catch (Exception ex)
{
    logger.Error(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    LogManager.Shutdown();
}