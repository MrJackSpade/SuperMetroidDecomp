namespace SuperMetroid.Core.Frontend;

/// <summary>Shared file-select, options and game-over selection-missile animation.</summary>
public static class MenuMissileAnimationDefinitions
{
    /// <summary>$82:BA80 masks the incremented animation frame to its four-frame cycle.</summary>
    public const int FrameCount = 4;

    /// <summary>
    /// $82:BAAA..BAB1 contains four identical eight-call timer reloads. Independently
    /// verified for #1165; this constant already replaces that separate logical table.
    /// The consumer decrements before advancing and may use editable presentation timing.
    /// </summary>
    public const int FrameDuration = 8;

    /// <summary>
    /// Returns the $82:BAB2 Draw_Menu_Selection_Missile spritemap ID for frame 0..3.
    /// </summary>
    /// <remarks>
    /// Independently reviewed for #1165 against the supported ROM and pinned bank_82.asm.
    /// The four consecutive menu spritemaps play in reverse ID order: 0x37-frame.
    /// Native indexing doubles the frame to select a word; wrap belongs to the caller.
    /// Reject unsupported indices with the former arrays' IndexOutOfRangeException,
    /// rather than masking them. All three menu views and extraction share this function.
    /// </remarks>
    public static ushort SpritemapId(int frame)
    {
        if ((uint)frame >= FrameCount) throw new IndexOutOfRangeException();
        return (ushort)(0x37 - frame);
    }
}
