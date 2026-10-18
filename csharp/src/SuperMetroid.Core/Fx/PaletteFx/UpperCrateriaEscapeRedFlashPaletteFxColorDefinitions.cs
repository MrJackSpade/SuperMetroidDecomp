namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable mechanics for upper Crateria's escape red-flash loop.</summary>
/// <remarks>
/// Definition <c>$FFE5</c> enters one fourteen-record program. Its 98 BGR555 words
/// remain live presentation data; this catalog owns palette placement, timing, waits,
/// and loop control.
/// </remarks>
public static class UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions
{
    /// <summary>The loop contains fourteen timed records.</summary>
    public const int FrameCount = 14;
    /// <summary>Each record writes seven live BGR555 colors.</summary>
    public const int ColorsPerFrame = 7;
    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = 18;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorRows Rows { get; } = new(611, FrameCount, ColorsPerFrame);

    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public static PaletteFxColorKey Color(int frame, int color) => Rows.Color(frame, color);
}
