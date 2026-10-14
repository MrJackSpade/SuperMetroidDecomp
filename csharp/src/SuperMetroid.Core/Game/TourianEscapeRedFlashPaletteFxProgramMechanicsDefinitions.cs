namespace SuperMetroid.Core.Game;

/// <summary>Immutable mechanics for the first two Tourian escape palette loops.</summary>
/// <remarks>
/// The shutter and background programs contain fourteen uniform records apiece. Their
/// 140 BGR555 words remain live presentation data; this catalog owns palette placement,
/// record timing, waits, and loop control.
/// </remarks>
public static class TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions
{
    /// <summary><c>PalFxDef_Tourian8</c> at <c>$8D:FFC9</c>.</summary>
    public const ushort ShutterDefinitionPointer = 0xffc9;

    /// <summary><c>PalFxInstList_Tourian8</c> at <c>$8D:F7A9</c>.</summary>
    public const ushort ShutterProgramStart = 0xf7a9;

    /// <summary><c>PalFxDef_Tourian10</c> at <c>$8D:FFCD</c>.</summary>
    public const ushort BackgroundDefinitionPointer = 0xffcd;

    /// <summary><c>PalFxInstList_Tourian10</c> at <c>$8D:F891</c>.</summary>
    public const ushort BackgroundProgramStart = 0xf891;

    /// <summary>Both loops contain fourteen timed records.</summary>
    public const int FrameCount = 14;

    /// <summary>Shutter and background loops with distinct CGRAM starts, frame durations, and color extents.</summary>
    private static readonly TourianEscapeRedFlashPaletteFxProgramDefinition[] Definitions =
    [
        new(TourianEscapeRedFlashPaletteOwner.Shutter,
            ShutterDefinitionPointer, ShutterProgramStart, 0x0132, 2, 6),
        new(TourianEscapeRedFlashPaletteOwner.Background,
            BackgroundDefinitionPointer, BackgroundProgramStart, 0x0070, 4, 4),
    ];

    /// <summary>The shutter and background red-flash programs in definition order.</summary>
    public static IReadOnlyList<TourianEscapeRedFlashPaletteFxProgramDefinition> All { get; } = Array.AsReadOnly(Definitions);

    /// <summary>Resolves one compiled word-sized mechanic across both programs.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        foreach (TourianEscapeRedFlashPaletteFxProgramDefinition definition in Definitions)
        {
            if (definition.TryReadMechanicsWord(pointer, out value))
                return true;
        }

        value = 0;
        return false;
    }
}
