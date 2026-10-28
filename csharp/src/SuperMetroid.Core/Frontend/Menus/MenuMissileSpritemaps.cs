namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>Selects the selection missile's spritemap for each animation frame (<see cref="MenuMissileAnimationDefinitions"/>).</summary>
public static class MenuMissileSpritemaps
{
    /// <summary>
    /// Returns the $82:BAB2 Draw_Menu_Selection_Missile spritemap ID for frame 0..3.
    /// </summary>
    /// <remarks>
    /// Independently reviewed for #1165 against the supported ROM and pinned bank_82.asm.
    /// The four consecutive menu spritemaps play in reverse ID order.
    /// Native indexing doubles the frame to select a word; wrap belongs to the caller.
    /// Reject unsupported indices with ArgumentOutOfRangeException,
    /// rather than masking them. All three menu views and extraction share this function.
    /// </remarks>
    public static ushort SpritemapId(int frame)
    {
        if ((uint)frame >= MenuMissileAnimationDefinitions.FrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
        return (ushort)(MenuMissileAnimationDefinitions.FirstFrameSpritemap - frame);
    }
}
