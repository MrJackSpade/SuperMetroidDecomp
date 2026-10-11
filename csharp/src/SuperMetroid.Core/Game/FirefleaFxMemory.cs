using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>WRAM state of FX $24, initialized by $88:B07C and stepped by its pre-instruction $88:B0BC (#629).</summary>
[CpuBusBoundary("catalog: Fireflea room FX WRAM")]
public static class FirefleaFxWorkRam
{
    /// <summary>$7E:1778 <c>FirefleaFlashing_Timer</c>, decremented only while time is not frozen.</summary>
    public static readonly CpuBusCell Timer = CpuBusCell.WramWord(nameof(Timer), 0x001778, "frame countdown (u16)");

    /// <summary>$7E:177A <c>FirefleaFlashing_Index</c>, a word-sized flashing-table element index.</summary>
    public static readonly CpuBusCell Index = CpuBusCell.WramWord(nameof(Index), 0x00177a, "flash table index (u16)");

    /// <summary>$7E:177E <c>FirefleaFlashing_DarknessLevel</c>, advanced by dying Firefleas.</summary>
    public static readonly CpuBusCell Darkness = CpuBusCell.WramWord(nameof(Darkness), 0x00177e, "darkness level (u16)");
}

/// <summary>Typed view of the Fireflea room FX WRAM state (#629).</summary>
[CpuBusBoundary("domain view: Fireflea room FX WRAM")]
public readonly struct FirefleaFxMemory(ISnesMemoryBus bus)
{
    /// <summary>Frames until the next flashing-index step.</summary>
    public ushort Timer
    {
        get => new WramWord(bus, FirefleaFxWorkRam.Timer).Read();
        set => new WramWord(bus, FirefleaFxWorkRam.Timer).Write(value);
    }

    /// <summary>The current flashing-table element index.</summary>
    public ushort Index
    {
        get => new WramWord(bus, FirefleaFxWorkRam.Index).Read();
        set => new WramWord(bus, FirefleaFxWorkRam.Index).Write(value);
    }

    /// <summary>The darkness level published from the enemy-owned death counter.</summary>
    public ushort Darkness
    {
        get => new WramWord(bus, FirefleaFxWorkRam.Darkness).Read();
        set => new WramWord(bus, FirefleaFxWorkRam.Darkness).Write(value);
    }
}
