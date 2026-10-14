namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Typed view of the X-ray setup WRAM buffers (#629).</summary>
[CpuBusBoundary("domain view: X-ray setup WRAM buffers")]
public readonly struct XrayMemory(ISnesMemoryBus bus)
{
    /// <summary>The two captured BG1 screens, first screen first.</summary>
    public WorkRamWordTable SavedBg1 => new(bus, XrayWorkRam.SavedBg1);

    /// <summary>The frozen reveal map; it is never rebuilt from later room or VRAM changes.</summary>
    public WorkRamWordTable RevealTilemap => new(bus, XrayWorkRam.RevealTilemap);
}
