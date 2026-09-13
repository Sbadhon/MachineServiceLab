using Microsoft.EntityFrameworkCore;

namespace MachineServiceLab.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<MachineEntity> Machines => Set<MachineEntity>();
    public DbSet<DiagnosticsEntity> Diagnostics => Set<DiagnosticsEntity>();
    public DbSet<TelemetryEntity> Telemetry => Set<TelemetryEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var machine = modelBuilder.Entity<MachineEntity>();

        machine.HasKey(x => x.SerialNumber);

        machine.Property(x => x.SerialNumber)
            .HasMaxLength(50);

        machine.Property(x => x.Model)
            .HasMaxLength(100);

        machine.Property(x => x.FirmwareVersion)
            .HasMaxLength(50);

        var diagnostics = modelBuilder.Entity<DiagnosticsEntity>();

        diagnostics.HasKey(x => x.Id);

        diagnostics.Property(x => x.SerialNumber)
            .HasMaxLength(50);

        diagnostics.HasOne<MachineEntity>()
            .WithMany()
            .HasForeignKey(x => x.SerialNumber)
            .OnDelete(DeleteBehavior.Cascade);

        diagnostics.HasIndex(x => new
        {
            x.SerialNumber,
            x.Id
        });

        var telemetry = modelBuilder.Entity<TelemetryEntity>();

        telemetry.HasKey(x => x.Id);

        telemetry.Property(x => x.SerialNumber)
            .HasMaxLength(50);

        telemetry.Property(x => x.Metric)
            .HasMaxLength(100);

        telemetry.Property(x => x.Unit)
            .HasMaxLength(20);

        telemetry.HasOne<MachineEntity>()
            .WithMany()
            .HasForeignKey(x => x.SerialNumber)
            .OnDelete(DeleteBehavior.Cascade);

        telemetry.HasIndex(x => new
        {
            x.SerialNumber,
            x.Id
        });
    }
}

public sealed class MachineEntity
{
    public required string SerialNumber { get; set; }
    public required string Model { get; set; }
    public required string FirmwareVersion { get; set; }
    public DateTimeOffset RegisteredAt { get; set; }
}

public sealed class DiagnosticsEntity
{
    public int Id { get; set; }

    public required string SerialNumber { get; set; }

    public int BatteryPercent { get; set; }
    public double BatteryVoltage { get; set; }
    public double ControllerTemperatureC { get; set; }
    public double MachineHours { get; set; }

    public required string FaultCodesJson { get; set; }

    public DateTimeOffset CapturedAt { get; set; }
}

public sealed class TelemetryEntity
{
    public int Id { get; set; }

    public required string SerialNumber { get; set; }
    public required string Metric { get; set; }

    public double Value { get; set; }

    public required string Unit { get; set; }

    public DateTimeOffset CapturedAt { get; set; }
}