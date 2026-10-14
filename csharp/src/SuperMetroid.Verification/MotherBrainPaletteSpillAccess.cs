using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Verification view of Mother Brain's grey palette-copy WRAM spill (#629). Production's
/// <see cref="MotherBrainPaletteMemoryDefinitions"/> only stores the word; verification reads it back.
/// </summary>
[CpuBusBoundary("domain view: Mother Brain palette-copy WRAM spill read back for verification")]
internal readonly struct MotherBrainPaletteSpillAccess(ISnesMemoryBus bus)
{
    /// <summary>The grey transition's trailing color word, as the cartridge stored it.</summary>
    internal ushort GreyTrailingWord => new WramWord(bus, MotherBrainPaletteWorkRam.GreyTrailingWord).Read();
}
