using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTrackingAPI.Models;

namespace TimeTrackingAPI.Data.Configurations
{
    /// <summary>
    /// Конфигурация сущности TimeEntry
    /// </summary>
    public class TimeEntryConfiguration : IEntityTypeConfiguration<TimeEntry>
    {
        public void Configure(EntityTypeBuilder<TimeEntry> builder)
        {
            // Настройка таблицы
            builder.ToTable("TimeEntries");

            // Первичный ключ
            builder.HasKey(timeEntry => timeEntry.Id);
            builder.Property(timeEntry => timeEntry.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("TimeEntryId");

            // Настройка свойств
            builder.Property(timeEntry => timeEntry.Date)
                .IsRequired()
                .HasColumnType("date")
                .HasColumnName("EntryDate");

            builder.Property(timeEntry => timeEntry.Hours)
                .IsRequired()
                .HasColumnType("decimal(4,2)")
                .HasColumnName("HoursWorked");

            builder.Property(timeEntry => timeEntry.Description)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("WorkDescription");

            builder.Property(timeEntry => timeEntry.TaskId)
                .IsRequired()
                .HasColumnName("TaskId");

            // Индексы для оптимизации запросов
            builder.HasIndex(timeEntry => timeEntry.Date)
                .HasDatabaseName("IX_TimeEntries_Date");

            builder.HasIndex(timeEntry => new { timeEntry.Date, timeEntry.TaskId })
                .HasDatabaseName("IX_TimeEntries_Date_TaskId");

            builder.HasIndex(timeEntry => timeEntry.TaskId)
                .HasDatabaseName("IX_TimeEntries_TaskId");

            // Отношения
            builder.HasOne(timeEntry => timeEntry.Task)
                .WithMany(workTask => workTask.TimeEntries)
                .HasForeignKey(timeEntry => timeEntry.TaskId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
