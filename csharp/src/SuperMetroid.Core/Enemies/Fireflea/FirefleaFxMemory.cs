namespace SuperMetroid.Core.Enemies.Fireflea;

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
