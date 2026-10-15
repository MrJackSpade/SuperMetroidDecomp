namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>WRAM state of FX $24, initialized by $88:B07C and stepped by its pre-instruction $88:B0BC (#629).</summary>
[CpuBusBoundary("catalog: Fireflea room FX WRAM")]
public static class FirefleaFxWorkRam
{
    /// <summary>$7E:1778 <c>FirefleaFlashing_Timer</c>, decremented only while time is not frozen.</summary>
    public static readonly CpuBusCell Timer = CpuBusCell.WramWord(nameof(Timer), 6008, "frame countdown (u16)");

    /// <summary>$7E:177A <c>FirefleaFlashing_Index</c>, a word-sized flashing-table element index.</summary>
    public static readonly CpuBusCell Index = CpuBusCell.WramWord(nameof(Index), 6010, "flash table index (u16)");

    /// <summary>$7E:177E <c>FirefleaFlashing_DarknessLevel</c>, advanced by dying Firefleas.</summary>
    public static readonly CpuBusCell Darkness = CpuBusCell.WramWord(nameof(Darkness), 6014, "darkness level (u16)");
}
