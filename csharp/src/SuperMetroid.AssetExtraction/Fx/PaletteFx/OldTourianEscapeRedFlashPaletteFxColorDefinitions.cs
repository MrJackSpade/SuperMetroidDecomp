using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>Immutable mechanics for the old-Tourian escape-shaft red-flash loop.</summary>
/// <remarks>
/// The definition enters one fourteen-record program. Its 112 BGR555 words
/// remain live presentation data; this catalog owns palette placement, timing, both
/// inline colour skips, waits, and loop control.
/// </remarks>
public static class OldTourianEscapeRedFlashPaletteFxColorDefinitions
{

    /// <summary>Each record writes eight live BGR555 colors.</summary>
    public const int ColorsPerFrame = 8;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(PaletteFxSegment.OldTourianRedFlashingCycle, ColorsPerFrame);
}
