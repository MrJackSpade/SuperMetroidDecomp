namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Immutable control words for Wrecked Ship's powered green-light palette loop.
/// </summary>
/// <remarks>
/// Palette-FX definitions <c>$F76D</c> and <c>$F771</c> share this program. Its sixteen
/// BGR555 colors remain live presentation data; only setup, timing, waits, and loop control
/// are compiled here.
/// $8D:EAE2 selects CGRAM byte $0098. Records f=0..7 begin at
/// $EAE6 + 8*f, last ten frames, write two live colors, and end in
/// $C595 wait. The $C61E goto at $EB26 returns to $EAE6 after an
/// 80-frame cycle; f=8 reaches control. All 20 mechanics words match
/// the pinned NTSC J/U v1.0 ROM for both definition entries.
/// </remarks>
public static class WreckedShipGreenLightPaletteFxColorDefinitions
{

    /// <summary>Eight ten-frame records form the powered-light cycle.</summary>
    public const int FrameCount = 8;

    /// <summary>Two BGR555 colors are presentation-owned by each record.</summary>
    public const int ColorsPerFrame = 2;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorRows Rows { get; } = new(347, FrameCount, ColorsPerFrame);
}
