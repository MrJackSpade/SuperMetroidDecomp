namespace SuperMetroid.Core.Frontend.Menus;

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

    /// <summary>$82:C5D1: the first menu missile spritemap identity; playback order comes from $82:BAB2, not atlas ordering.</summary>
    internal const int FirstNativeSpritemap = 0x34;

    /// <summary>$82:CBD7: the second missile pose's left cell, the top-left of the $EE/$EF/$FE/$FF two-by-two atlas block.</summary>
    internal const int SecondPoseLeftCell = 0xee;

    /// <summary>$82:CBE3: the third missile pose's right cell.</summary>
    internal const int ThirdPoseRightCell = 0xcc;

    /// <summary>$82:CBEF: the final missile pose's left cell; native part order is left then right.</summary>
    internal const int FinalPoseLeftCell = 0xc8;
}
