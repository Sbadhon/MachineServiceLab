using Microsoft.EntityFrameworkCore;

namespace MachineServiceLab.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<MachineEntity> Machines => Set<MachineEntity>();
    public DbSet<DiagnosticsEntity> Diagnostics => Set<DiagnosticsEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MachineEntity>()
            .HasKey(x => x.SerialNumber);
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