namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable mechanics for the first two Tourian escape palette loops.</summary>
/// <remarks>
/// The shutter and background programs contain fourteen uniform records apiece. Their
/// 140 BGR555 words remain live presentation data; this catalog owns palette placement,
/// record timing, waits, and loop control.
/// </remarks>
public static class TourianEscapeRedFlashPaletteFxColorDefinitions
{

    /// <summary>Both loops contain fourteen timed records.</summary>
    public const int FrameCount = 14;

    /// <summary>Shutter and background loops with distinct CGRAM starts, frame durations, and color extents.</summary>
    private static readonly TourianEscapeRedFlashPaletteFxColorDefinition[] Definitions =
    [
        new(TourianEscapeRedFlashPaletteOwner.Shutter,
            PaletteFxObject.TourianShutterRedFlashing, 525, 0x0132, 2, 6),
        new(TourianEscapeRedFlashPaletteOwner.Background,
            PaletteFxObject.TourianBackgroundRedFlashing, 539, 0x0070, 4, 4),
    ];

    /// <summary>The shutter and background red-flash programs in definition order.</summary>
    public static IReadOnlyList<TourianEscapeRedFlashPaletteFxColorDefinition> All { get; } = Array.AsReadOnly(Definitions);
}
