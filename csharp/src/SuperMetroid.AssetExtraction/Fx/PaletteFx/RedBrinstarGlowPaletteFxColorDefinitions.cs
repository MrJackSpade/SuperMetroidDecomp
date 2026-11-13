using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>Immutable control words for Red Brinstar's background-glow palette loop.</summary>
/// <remarks>
/// The palette-FX definition owns fourteen timed records.
/// Its 112 BGR555 colors remain live presentation data; this catalog owns only color-index
/// setup, durations, waits, and loop control.
/// </remarks>
public static class RedBrinstarGlowPaletteFxColorDefinitions
{

    /// <summary>Eight BGR555 colors are presentation-owned by each timed record.</summary>
    public const int ColorsPerFrame = 8;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(PaletteFxSegment.BrinstarRedBackgroundGlowCycle, ColorsPerFrame);
}
