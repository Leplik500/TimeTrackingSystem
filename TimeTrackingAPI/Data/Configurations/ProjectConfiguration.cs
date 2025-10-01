using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTrackingAPI.Models;

namespace TimeTrackingAPI.Data.Configurations;

/// <summary>
///     Конфигурация сущности Project
/// </summary>
public class
        ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        // Настройка таблицы
        builder.ToTable("Projects");

        // Первичный ключ
        builder.HasKey(project => project.Id);
        builder.Property(project => project.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ProjectId");

        // Настройка свойств
        builder.Property(project => project.Name)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("ProjectName");

        builder.Property(project => project.Code)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("ProjectCode");

        builder.Property(project => project.IsActive)
                .IsRequired()
                .HasDefaultValue(true)
                .HasColumnName("IsActive");

        // Индексы
        builder.HasIndex(project => project.Code)
                .IsUnique()
                .HasDatabaseName("IX_Projects_Code");

        builder.HasIndex(project => project.Name)
                .HasDatabaseName("IX_Projects_Name");

        // Отношения
        builder.HasMany(project => project.Tasks)
                .WithOne(workTask => workTask.Project)
                .HasForeignKey(workTask => workTask.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);
    }
}