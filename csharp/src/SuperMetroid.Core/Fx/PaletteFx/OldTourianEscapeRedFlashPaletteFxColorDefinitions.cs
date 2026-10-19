namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable mechanics for the old-Tourian escape-shaft red-flash loop.</summary>
/// <remarks>
/// Definition <c>$FFD9</c> enters one fourteen-record program. Its 112 BGR555 words
/// remain live presentation data; this catalog owns palette placement, timing, both
/// inline CGRAM skips, waits, and loop control.
/// </remarks>
public static class OldTourianEscapeRedFlashPaletteFxColorDefinitions
{

    /// <summary>The loop contains fourteen timed records.</summary>
    public const int FrameCount = 14;

    /// <summary>Each record writes eight live BGR555 colors.</summary>
    public const int ColorsPerFrame = 8;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorRows Rows { get; } = new(567, FrameCount, ColorsPerFrame);
}
