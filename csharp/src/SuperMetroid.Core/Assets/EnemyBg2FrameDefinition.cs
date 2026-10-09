namespace SuperMetroid.Core.Assets;

/// <summary>A fixed enemy BG2 extended-frame identity with an editable visual name.</summary>
/// <param name="Pointer">Native instruction selector that identifies this BG2 frame.</param>
/// <param name="Name">Stable editable key used to find the frame in its installed visual document.</param>
internal readonly record struct EnemyBg2FrameDefinition(ushort Pointer, string Name);

/// <summary>Shared dimensions of the enemy BG2 tilemap targeted by $A0:96CA.</summary>
internal static class EnemyBg2FrameLayout
{
    /// <summary>$FFFE marks an extended spritemap component as a BG2 command stream.</summary>
    internal const ushort StreamMarker = 0xfffe;
    /// <summary>First VRAM word of the enemy BG2 tilemap.</summary>
    internal const ushort VramBase = 0x4800;
    /// <summary>First byte of the native $7E working tilemap.</summary>
    internal const ushort WorkingRamBase = 0x2000;
    /// <summary>Number of BG2 tilemap words in one row of the native working tilemap.</summary>
    internal const int TilemapWidth = 32;

    /// <summary>Number of rows in the native BG2 working tilemap.</summary>
    internal const int TilemapHeight = 64;
    /// <summary>The largest installed mixed root has ten native components (Mother Brain).</summary>
    internal const int MaximumComponents = MotherBrainBodyVisualDefinitions.MaximumNativeComponents;
    /// <summary>Defensive bound for one $FFFE stream's terminated command list.</summary>
    internal const int MaximumCommandsPerStream = 128;
}
