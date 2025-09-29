using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTrackingAPI.Models;

namespace TimeTrackingAPI.Data.Configurations
{
    /// <summary>
    /// Конфигурация сущности WorkTask
    /// </summary>
    public class WorkTaskConfiguration : IEntityTypeConfiguration<WorkTask>
    {
        public void Configure(EntityTypeBuilder<WorkTask> builder)
        {
            // Настройка таблицы
            builder.ToTable("WorkTasks");

            // Первичный ключ
            builder.HasKey(workTask => workTask.Id);
            builder.Property(workTask => workTask.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("TaskId");

            // Настройка свойств
            builder.Property(workTask => workTask.Name)
                .IsRequired()
                .HasMaxLength(300)
                .HasColumnName("TaskName");

            builder.Property(workTask => workTask.ProjectId)
                .IsRequired()
                .HasColumnName("ProjectId");

            builder.Property(workTask => workTask.IsActive)
                .IsRequired()
                .HasDefaultValue(true)
                .HasColumnName("IsActive");

            // Индексы
            builder.HasIndex(workTask => new { workTask.Name, workTask.ProjectId })
                .IsUnique()
                .HasDatabaseName("IX_WorkTasks_Name_ProjectId");

            builder.HasIndex(workTask => workTask.ProjectId)
                .HasDatabaseName("IX_WorkTasks_ProjectId");

            builder.HasIndex(workTask => workTask.IsActive)
                .HasDatabaseName("IX_WorkTasks_IsActive");

            // Отношения
            builder.HasOne(workTask => workTask.Project)
                .WithMany(project => project.Tasks)
                .HasForeignKey(workTask => workTask.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(workTask => workTask.TimeEntries)
                .WithOne(timeEntry => timeEntry.Task)
                .HasForeignKey(timeEntry => timeEntry.TaskId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
