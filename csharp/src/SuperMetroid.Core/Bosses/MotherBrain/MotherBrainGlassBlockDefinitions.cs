namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The level-word values the Mother Brain glass PLM installs and checks.</summary>
internal static class MotherBrainGlassBlockDefinitions
{
    /// <summary>The room argument of the glass's population record.</summary>
    internal const ushort GlassRoomArgument = 0x8000;

    /// <summary>The tile-number bits of a level word, which the glass keeps.</summary>
    internal const ushort TileNumberBits = 0x0fff;

    /// <summary>Block type 8, solid, which the glass gives its block.</summary>
    internal const ushort GlassBlockType = 0x8000;
}
