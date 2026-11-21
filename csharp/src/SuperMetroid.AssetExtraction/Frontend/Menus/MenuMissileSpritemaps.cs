using SuperMetroid.Core.Frontend.Menus;

namespace SuperMetroid.AssetExtraction.Frontend.Menus;

/// <summary>Selects the selection missile's spritemap for each animation frame (<see cref="MenuMissileAnimationDefinitions"/>).</summary>
public static partial class MenuMissileSpritemaps
{
    /// <summary>
    /// Returns the $82:BAB2 Draw_Menu_Selection_Missile spritemap ID for <paramref name="frame"/>, by its place in the roll.
    /// </summary>
    /// <remarks>
    /// Independently reviewed for #1165 against the supported ROM and pinned bank_82.asm.
    /// The four consecutive menu spritemaps play in reverse ID order.
    /// Native indexing doubles the frame to select a word; wrap belongs to the caller.
    /// Reject unsupported indices with ArgumentOutOfRangeException,
    /// rather than masking them. Extraction reads each frame's spritemap with it.
    /// </remarks>
    internal static ushort SpritemapId(MenuMissileFrame frame)
    {
        int place = MenuMissileAnimationDefinitions.Roll.ToList().IndexOf(frame);
        if (place < 0) throw new ArgumentOutOfRangeException(nameof(frame));
        return (ushort)(MenuMissileSpriteDefinitions.FirstFrameSpritemap - place);
    }
}
