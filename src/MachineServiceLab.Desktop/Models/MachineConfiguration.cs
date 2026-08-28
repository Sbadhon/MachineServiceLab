namespace MachineServiceLab.Desktop.Models;

public sealed record MachineConfiguration(
    bool EcoMode,
    int BrushPressureLevel,
    int MaxSpeedPercent);