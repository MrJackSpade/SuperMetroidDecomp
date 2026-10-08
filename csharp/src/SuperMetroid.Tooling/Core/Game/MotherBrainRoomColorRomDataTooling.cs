namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MotherBrainRoomColorRomData"/>; never linked by player hosts.</summary>
internal static class MotherBrainRoomColorRomDataTooling
{
    /// <summary>The imported palette-pointer operand follows the timed entry's duration word.</summary>
    public const int PaletteOperandByteOffset = sizeof(ushort);
}
