namespace SuperMetroid.Core.Game;

/// <summary>Typed view of the WRAM word Mother Brain's grey palette copies spill into (#629).</summary>
[CpuBusBoundary("domain view: Mother Brain palette-copy WRAM spill")]
public readonly struct MotherBrainPaletteMemoryDefinitions(ISnesMemoryBus bus)
{
    /// <summary>The grey transition's trailing color, stored low byte first.</summary>
    public Bgr555 GreyTrailingWord
    {
        set => new WramWord(bus, MotherBrainPaletteWorkRam.GreyTrailingWord).Write(value.ToWord());
    }
}
