namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Development-tool members of <see cref="MotherBrainRoomColorRomData"/>; never linked by player hosts.</summary>
internal static class MotherBrainRoomColorRomDataTooling
{
    /// <summary>The imported palette-pointer operand follows the timed entry's duration word.</summary>
    public const int PaletteOperandByteOffset = sizeof(ushort);
    /// <summary>Each $A9:D046 timed flash entry contains a duration word and a palette-pointer word.</summary>
    public const int TimedEntryByteCount = 2 * sizeof(ushort);
}
