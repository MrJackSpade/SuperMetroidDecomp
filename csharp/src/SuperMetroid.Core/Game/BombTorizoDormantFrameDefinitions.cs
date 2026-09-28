namespace SuperMetroid.Core.Game;

/// <summary>
/// Engine-owned collision identity for the initial Bomb/Golden Torizo blank
/// extended frame. Its editable OAM component is held in the art catalog.
/// </summary>
internal static class BombTorizoDormantFrameDefinitions
{
    /// <summary>Bank of the shared Torizo extended frame and hitbox.</summary>
    internal const byte Bank = 0xaa;

    /// <summary><c>ExtSpritemap_Torizo_Blank</c> at $AA:87D0.</summary>
    internal const ushort Frame = 0x87d0;

    /// <summary><c>Hitbox_Torizo_Blank</c> at $AA:87C7, containing zero boxes.</summary>
    internal const ushort EmptyHitboxList = 0x87c7;

    internal const ushort ComponentCount = 1;
    internal const ushort HitboxCount = 0;
}
